// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract partial class ExpressionOrPatternSyntax : CSharpSyntaxNode
{
    internal ExpressionOrPatternSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    { }
}
/// <summary>Provides the base class from which the classes that represent expression syntax nodes are derived. This is an abstract class.</summary>
internal abstract partial class ExpressionSyntax : ExpressionOrPatternSyntax
{
    internal ExpressionSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    { }
}
