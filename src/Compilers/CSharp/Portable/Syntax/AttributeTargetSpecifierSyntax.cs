﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp.Syntax
{
    public sealed partial class AttributeTargetSpecifierSyntax : CSharpSyntaxNode
    {
        internal AttributeLocation GetAttributeLocation()
        {
            return this.Identifier.ToAttributeLocation();
        }
    }
}
