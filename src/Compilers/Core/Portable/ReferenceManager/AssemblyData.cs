﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Diagnostics;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis
{
    internal partial class CommonReferenceManager<TCompilation, TAssemblySymbol>
    {
        /// <summary>
        /// Information about an assembly, used as an input for the Binder class.
        /// </summary>
        [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
        internal abstract class AssemblyData
        {
            /// <summary>
            /// Identity of the assembly.
            /// </summary>
            public abstract AssemblyIdentity Identity { get; }

            /// <summary>
            /// Identity of assemblies referenced by this assembly.
            /// References should always be returned in the same order.
            /// </summary>
            public abstract ImmutableArray<AssemblyIdentity> AssemblyReferences { get; }

            /// <summary>
            /// The sequence of AssemblySymbols the Binder can choose from.
            /// </summary>
            public abstract ImmutableArray<TAssemblySymbol> AvailableSymbols { get; }

            /// <summary>
            /// Check if provided AssemblySymbol is created for assembly described by this instance. 
            /// This method is expected to return true for every AssemblySymbol returned by 
            /// AvailableSymbols property.
            /// </summary>
            /// <param name="assembly">
            /// The AssemblySymbol to check.
            /// </param>
            /// <returns>Boolean.</returns>
            public abstract bool IsMatchingAssembly(TAssemblySymbol? assembly);

            /// <summary>
            /// Resolve assembly references against assemblies described by provided AssemblyData objects. 
            /// In other words, match assembly identities returned by AssemblyReferences property against 
            /// assemblies described by provided AssemblyData objects.
            /// </summary>
            /// <param name="assemblies">AssemblyData objects to match against.</param>
            /// <param name="assemblyIdentityComparer">Used to compare assembly identities.</param>
            /// <returns>
            /// For each assembly referenced by this assembly (<see cref="AssemblyReferences"/>) 
            /// a description of how it binds to one of the input assemblies.
            /// </returns>
            public abstract AssemblyReferenceBinding[] BindAssemblyReferences(MultiDictionary<string, (AssemblyData DefinitionData, int DefinitionIndex)> assemblies, AssemblyIdentityComparer assemblyIdentityComparer);

            public abstract bool ContainsNoPiaLocalTypes { get; }

            public abstract bool IsLinked { get; }

            public abstract bool DeclaresTheObjectClass { get; }

            /// <summary>
            /// Get the source compilation backing this assembly, if one exists.
            /// Returns null otherwise.
            /// </summary>
            public abstract Compilation? SourceCompilation { get; }

            private string GetDebuggerDisplay() => $"{GetType().Name}: [{Identity.GetDisplayName()}]";
#if DEBUG
            public sealed override bool Equals(object? obj)
            {
                return base.Equals(obj);
            }

            public sealed override int GetHashCode()
            {
                return base.GetHashCode();
            }
#endif
        }
    }
}
