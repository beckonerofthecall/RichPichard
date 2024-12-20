﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Scripting.Hosting
{
    /// <summary>
    /// Implements an assembly loader for interactive compiler and REPL.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The class is thread-safe.
    /// </para>
    /// </remarks>
    public sealed partial class InteractiveAssemblyLoader : IDisposable
    {
        private class LoadedAssembly
        {
            /// <summary>
            /// The original path of the assembly before it was shadow-copied.
            /// For GAC'd assemblies, this is equal to Assembly.Location no matter what path was used to load them.
            /// </summary>
            public string? OriginalPath { get; set; }
        }

        private readonly AssemblyLoaderImpl _runtimeAssemblyLoader;

        private readonly MetadataShadowCopyProvider? _shadowCopyProvider;

        // Synchronizes assembly reference tracking.
        // Needs to be thread-safe since assembly loading may be triggered at any time by CLR type loader.
        private readonly object _referencesLock = new object();

        // loaded assembly -> loaded assembly info
        private readonly Dictionary<Assembly, LoadedAssembly> _assembliesLoadedFromLocation;

        // { original full path -> assembly }
        // Note, that there might be multiple entries for a single GAC'd assembly.
        private readonly Dictionary<string, AssemblyAndLocation> _assembliesLoadedFromLocationByFullPath;

        // simple name -> loaded assemblies
        private readonly Dictionary<string, List<LoadedAssemblyInfo>> _loadedAssembliesBySimpleName;

        // simple name -> identity and location of a known dependency
        private readonly Dictionary<string, List<AssemblyIdentityAndLocation>> _dependenciesWithLocationBySimpleName;

        [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
        private readonly struct AssemblyIdentityAndLocation
        {
            public readonly AssemblyIdentity Identity;
            public readonly string Location;

            public AssemblyIdentityAndLocation(AssemblyIdentity identity, string location)
            {
                Identity = identity;
                Location = location;
            }

            private string GetDebuggerDisplay() => Identity + " @ " + Location;
        }

        [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
        private readonly struct LoadedAssemblyInfo(Assembly assembly, AssemblyIdentity identity, string? location)
        {
            public readonly Assembly Assembly = assembly;
            public readonly AssemblyIdentity Identity = identity;
            public readonly string? Location = location;

            public bool IsDefault => Assembly == null;

            private string GetDebuggerDisplay() => IsDefault ? "uninitialized" : Identity.GetDisplayName() + (Location != null ? " @ " + Location : "");
        }

        public InteractiveAssemblyLoader(MetadataShadowCopyProvider? shadowCopyProvider = null)
        {
            _shadowCopyProvider = shadowCopyProvider;

            _assembliesLoadedFromLocationByFullPath = new Dictionary<string, AssemblyAndLocation>();
            _assembliesLoadedFromLocation = new Dictionary<Assembly, LoadedAssembly>();
            _loadedAssembliesBySimpleName = new Dictionary<string, List<LoadedAssemblyInfo>>(AssemblyIdentityComparer.SimpleNameComparer);
            _dependenciesWithLocationBySimpleName = new Dictionary<string, List<AssemblyIdentityAndLocation>>();

            _runtimeAssemblyLoader = AssemblyLoaderImpl.Create(this);
        }

        public void Dispose()
        {
            _runtimeAssemblyLoader.Dispose();
        }

        internal Assembly LoadAssemblyFromStream(Stream peStream, Stream pdbStream)
        {
            Assembly assembly = _runtimeAssemblyLoader.LoadFromStream(peStream, pdbStream);
            RegisterDependency(assembly);
            return assembly;
        }

        private AssemblyAndLocation Load(string reference)
        {
            MetadataShadowCopy? copy = null;
            try
            {
                if (_shadowCopyProvider != null)
                {
                    // All modules of the assembly has to be copied at once to keep the metadata consistent.
                    // This API copies the xml doc file and keeps the memory-maps of the metadata.
                    // We don't need that here but presumably the provider is shared with the compilation API that needs both.
                    // Ideally the CLR would expose API to load Assembly given a byte* and not create their own memory-map.
                    copy = _shadowCopyProvider.GetMetadataShadowCopy(reference, MetadataImageKind.Assembly);
                }

                var result = _runtimeAssemblyLoader.LoadFromPath((copy != null) ? copy.PrimaryModule.FullPath : reference);

                if (_shadowCopyProvider != null && result.GlobalAssemblyCache)
                {
                    _shadowCopyProvider.SuppressShadowCopy(reference);
                }

                return result;
            }
            catch (FileNotFoundException)
            {
                return default;
            }
            finally
            {
                // copy holds on the file handle, we need to keep the handle 
                // open until the file is locked by the CLR assembly loader:
                copy?.DisposeFileHandles();
            }
        }

        /// <summary>
        /// Notifies the assembly loader about a dependency that might be loaded in future.
        /// </summary>
        /// <param name="dependency">Assembly identity.</param>
        /// <param name="path">Assembly location.</param>
        /// <remarks>
        /// Associates a full assembly name with its location. The association is used when an assembly 
        /// is being loaded and its name needs to be resolved to a location.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="dependency"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is not an existing path.</exception>
        public void RegisterDependency(AssemblyIdentity dependency, string path)
        {
            if (dependency == null)
            {
                throw new ArgumentNullException(nameof(dependency));
            }

            if (!PathUtilities.IsAbsolute(path))
            {
                throw new ArgumentException(ScriptingResources.AbsolutePathExpected, nameof(path));
            }

            lock (_referencesLock)
            {
                RegisterDependencyNoLock(new AssemblyIdentityAndLocation(dependency, path));
            }
        }

        /// <summary>
        /// Notifies the assembly loader about an in-memory dependency that should be available within the resolution context.
        /// </summary>
        /// <param name="dependency">Assembly identity.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dependency"/> is null.</exception>
        /// <remarks>
        /// When another in-memory assembly references the <paramref name="dependency"/> the loader 
        /// responds with the specified dependency if the assembly identity matches the requested one.
        /// </remarks>
        public void RegisterDependency(Assembly dependency)
        {
            if (dependency == null)
            {
                throw new ArgumentNullException(nameof(dependency));
            }

            lock (_referencesLock)
            {
                RegisterLoadedAssemblySimpleNameNoLock(dependency, location: null);
            }
        }

        private void RegisterLoadedAssemblySimpleNameNoLock(Assembly assembly, string? location)
        {
            var identity = AssemblyIdentity.FromAssemblyDefinition(assembly);
            var info = new LoadedAssemblyInfo(assembly, identity, location);

            if (_loadedAssembliesBySimpleName.TryGetValue(identity.Name, out var existingInfos))
            {
                existingInfos.Add(info);
            }
            else
            {
                _loadedAssembliesBySimpleName.Add(identity.Name, new List<LoadedAssemblyInfo> { info });
            }
        }

        private void RegisterDependencyNoLock(AssemblyIdentityAndLocation dependency)
        {
            string simpleName = dependency.Identity.Name;
            if (_dependenciesWithLocationBySimpleName.TryGetValue(simpleName, out var sameSimpleNameAssemblyIdentities))
            {
                sameSimpleNameAssemblyIdentities.Add(dependency);
            }
            else
            {
                _dependenciesWithLocationBySimpleName.Add(simpleName, new List<AssemblyIdentityAndLocation> { dependency });
            }
        }

        internal Assembly? ResolveAssembly(string assemblyDisplayName, Assembly? requestingAssembly)
        {
            if (!AssemblyIdentity.TryParseDisplayName(assemblyDisplayName, out var identity))
            {
                return null;
            }

            string? loadDirectory;
            lock (_referencesLock)
            {
                if (requestingAssembly != null &&
                    _assembliesLoadedFromLocation.TryGetValue(requestingAssembly, out var loadedAssembly))
                {
                    loadDirectory = Path.GetDirectoryName(loadedAssembly.OriginalPath);
                }
                else
                {
                    loadDirectory = null;
                }
            }

            return ResolveAssembly(identity, loadDirectory);
        }

        internal Assembly? ResolveAssembly(AssemblyIdentity identity, string? loadDirectory)
        {
            // if the referring assembly is already loaded by our loader, load from its directory:
            if (loadDirectory != null)
            {
                Assembly? assembly;
                var conflictingLoadedAssemblyOpt = default(LoadedAssemblyInfo);
                var loadedAssemblyWithEqualNameAndVersionOpt = default(LoadedAssemblyInfo);

                lock (_referencesLock)
                {
                    // Has the file already been loaded?
                    assembly = TryGetAssemblyLoadedFromPath(identity, loadDirectory);
                    if (assembly != null)
                    {
                        return assembly;
                    }

                    // Has an assembly with the same name and version been loaded (possibly from a different directory)?
                    if (_loadedAssembliesBySimpleName.TryGetValue(identity.Name, out var loadedInfos))
                    {
                        // Desktop FX: A weak-named assembly conflicts with another weak-named assembly of the same simple name,
                        // unless we find an assembly whose identity matches exactly and whose content is exactly the same.
                        // TODO: We shouldn't block this on CoreCLR. https://github.com/dotnet/roslyn/issues/38621

                        if (!identity.IsStrongName)
                        {
                            conflictingLoadedAssemblyOpt = loadedInfos.FirstOrDefault(info => !info.Identity.IsStrongName);
                        }

                        loadedAssemblyWithEqualNameAndVersionOpt = loadedInfos.FirstOrDefault(info =>
                            AssemblyIdentityComparer.SimpleNameComparer.Equals(info.Identity.Name, identity.Name) &&
                            info.Identity.Version == identity.Version);
                    }
                }

                var assemblyFilePath = FindExistingAssemblyFile(identity.Name, loadDirectory);
                if (assemblyFilePath != null)
                {
                    // TODO: Stop using reflection once ModuleVersionId property once is available in Core contract.
                    if (loadedAssemblyWithEqualNameAndVersionOpt.Assembly != null)
                    {
                        if (TryReadMvid(assemblyFilePath, out var mvid) &&
                            loadedAssemblyWithEqualNameAndVersionOpt.Assembly.ManifestModule.ModuleVersionId == mvid)
                        {
                            return loadedAssemblyWithEqualNameAndVersionOpt.Assembly;
                        }

                        // error: attempt to load an assembly with the same identity as already loaded assembly but different content
                        throw new InteractiveAssemblyLoaderException(
                            string.Format(null, ScriptingResources.AssemblyAlreadyLoaded,
                            identity.Name,
                            identity.Version,
                            loadedAssemblyWithEqualNameAndVersionOpt.Location,
                            assemblyFilePath)
                        );
                    }

                    // TODO: Desktop FX only https://github.com/dotnet/roslyn/issues/38621
                    if (!conflictingLoadedAssemblyOpt.IsDefault)
                    {
                        // error: attempt to load an assembly with the same identity as already loaded assembly but different content
                        throw new InteractiveAssemblyLoaderException(
                            string.Format(null, ScriptingResources.AssemblyAlreadyLoadedNotSigned,
                            identity.Name,
                            conflictingLoadedAssemblyOpt.Location,
                            assemblyFilePath)
                        );
                    }

                    assembly = ShadowCopyAndLoadDependency(assemblyFilePath).Assembly;
                    if (assembly != null)
                    {
                        return assembly;
                    }
                }
            }

            return GetOrLoadKnownAssembly(identity);
        }

        private static string? FindExistingAssemblyFile(string simpleName, string directory)
        {
            string pathWithoutExtension = Path.Combine(directory, simpleName);
            foreach (var extension in RuntimeMetadataReferenceResolver.AssemblyExtensions)
            {
                string path = pathWithoutExtension + extension;
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private Assembly? TryGetAssemblyLoadedFromPath(AssemblyIdentity identity, string directory)
        {
            string pathWithoutExtension = Path.Combine(directory, identity.Name);

            foreach (var extension in RuntimeMetadataReferenceResolver.AssemblyExtensions)
            {
                AssemblyAndLocation assemblyAndLocation;
                if (_assembliesLoadedFromLocationByFullPath.TryGetValue(pathWithoutExtension + extension, out assemblyAndLocation) &&
                    identity.Equals(AssemblyIdentity.FromAssemblyDefinition(assemblyAndLocation.Assembly)))
                {
                    return assemblyAndLocation.Assembly;
                }
            }

            return null;
        }

        private static bool TryReadMvid(string filePath, out Guid mvid)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    using (var peReader = new PEReader(stream))
                    {
                        var metadataReader = peReader.GetMetadataReader();
                        mvid = metadataReader.GetGuid(metadataReader.GetModuleDefinition().Mvid);
                        return true;
                    }
                }
            }
            catch
            {
                mvid = default;
                return false;
            }
        }

        private Assembly? GetOrLoadKnownAssembly(AssemblyIdentity identity)
        {
            Assembly? assembly = null;
            string? assemblyFileToLoad = null;

            // Try to find the assembly among assemblies that we loaded, comparing its identity with the requested one.
            lock (_referencesLock)
            {
                // already loaded assemblies:
                if (_loadedAssembliesBySimpleName.TryGetValue(identity.Name, out var infos))
                {
                    assembly = FindHighestVersionOrFirstMatchingIdentity(identity, infos);
                    if (assembly != null)
                    {
                        return assembly;
                    }
                }

                // names:
                if (_dependenciesWithLocationBySimpleName.TryGetValue(identity.Name, out var sameSimpleNameIdentities))
                {
                    var identityAndLocation = FindHighestVersionOrFirstMatchingIdentity(identity, sameSimpleNameIdentities);
                    if (identityAndLocation.Identity != null)
                    {
                        assemblyFileToLoad = identityAndLocation.Location;
                        AssemblyAndLocation assemblyAndLocation;
                        if (_assembliesLoadedFromLocationByFullPath.TryGetValue(assemblyFileToLoad, out assemblyAndLocation))
                        {
                            return assemblyAndLocation.Assembly;
                        }
                    }
                }
            }

            if (assemblyFileToLoad != null)
            {
                assembly = ShadowCopyAndLoadDependency(assemblyFileToLoad).Assembly;
            }

            return assembly;
        }

        private AssemblyAndLocation ShadowCopyAndLoadDependency(string originalPath)
        {
            AssemblyAndLocation assemblyAndLocation = Load(originalPath);
            if (assemblyAndLocation.IsDefault)
            {
                return default;
            }

            lock (_referencesLock)
            {
                // Always remember the path. The assembly might have been loaded from another path or not loaded yet.
                _assembliesLoadedFromLocationByFullPath[originalPath] = assemblyAndLocation;

                if (_assembliesLoadedFromLocation.TryGetValue(assemblyAndLocation.Assembly, out var loadedAssembly))
                {
                    return assemblyAndLocation;
                }

                _assembliesLoadedFromLocation.Add(
                    assemblyAndLocation.Assembly,
                    new LoadedAssembly { OriginalPath = assemblyAndLocation.GlobalAssemblyCache ? assemblyAndLocation.Location : originalPath });

                RegisterLoadedAssemblySimpleNameNoLock(assemblyAndLocation.Assembly, assemblyAndLocation.Location);
            }

            return assemblyAndLocation;
        }

        private static Assembly? FindHighestVersionOrFirstMatchingIdentity(AssemblyIdentity identity, IEnumerable<LoadedAssemblyInfo> infos)
        {
            Assembly? candidate = null;
            Version? candidateVersion = null;
            foreach (var info in infos)
            {
                if (DesktopAssemblyIdentityComparer.Default.ReferenceMatchesDefinition(identity, info.Identity))
                {
                    if (candidate == null || candidateVersion < info.Identity.Version)
                    {
                        candidate = info.Assembly;
                        candidateVersion = info.Identity.Version;
                    }
                }
            }

            return candidate;
        }

        private static AssemblyIdentityAndLocation FindHighestVersionOrFirstMatchingIdentity(AssemblyIdentity identity, IEnumerable<AssemblyIdentityAndLocation> assemblies)
        {
            var candidate = default(AssemblyIdentityAndLocation);
            foreach (var assembly in assemblies)
            {
                if (DesktopAssemblyIdentityComparer.Default.ReferenceMatchesDefinition(identity, assembly.Identity))
                {
                    if (candidate.Identity == null || candidate.Identity.Version < assembly.Identity.Version)
                    {
                        candidate = assembly;
                    }
                }
            }

            return candidate;
        }
    }
}
