﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using Microsoft.CodeAnalysis.CommandLine;

namespace Microsoft.CodeAnalysis.CompilerServer
{
    internal interface ICompilerServerHost
    {
        ICompilerServerLogger Logger { get; }

        BuildResponse RunCompilation(in RunRequest request, CancellationToken cancellationToken);
    }
}
