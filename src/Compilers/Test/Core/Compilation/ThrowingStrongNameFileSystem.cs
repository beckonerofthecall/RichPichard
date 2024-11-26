// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.IO;
using Microsoft.CodeAnalysis;

namespace Roslyn.Test.Utilities
{
    internal sealed class ThrowingStrongNameFileSystem : StrongNameFileSystem
    {
        internal static new readonly ThrowingStrongNameFileSystem Instance = new ThrowingStrongNameFileSystem();

        internal override bool FileExists(string fullPath) => throw new IOException();

        internal override byte[] ReadAllBytes(string fullPath) => throw new IOException();
    }
}
