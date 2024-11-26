// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

/// <summary>Provides the base class from which the classes that represent name syntax nodes are derived. This is an abstract class.</summary>
internal abstract partial class NameSyntax : TypeSyntax
{
    internal NameSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    { }
}

/// <summary>Provides the base class from which the classes that represent simple name syntax nodes are derived. This is an abstract class.</summary>
internal abstract partial class SimpleNameSyntax : NameSyntax
{
    internal SimpleNameSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    { }

    /// <summary>SyntaxToken representing the identifier of the simple name.</summary>
    public abstract SyntaxToken Identifier { get; }
}

/// <summary>Class which represents the syntax node for identifier name.</summary>
internal sealed partial class IdentifierNameSyntax : SimpleNameSyntax
{
    internal readonly SyntaxToken identifier;

    internal IdentifierNameSyntax(SyntaxKind kind, SyntaxToken identifier, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        context?.Set(this);
        this.SlotCount = 1;
        this.AdjustFlagsAndWidth(identifier);
        this.identifier = identifier;
    }

    /// <summary>SyntaxToken representing the keyword for the kind of the identifier name.</summary>
    public override SyntaxToken Identifier => this.identifier;

    internal override GreenNode? GetSlot(int index)
        => index == 0 ? this.identifier : null;

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.IdentifierNameSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitIdentifierName(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitIdentifierName(this);

    public IdentifierNameSyntax Update(SyntaxToken identifier)
    {
        if (identifier != this.Identifier)
        {
            var newNode = SyntaxFactory.IdentifierName(identifier);
            var diags = GetDiagnostics();
            if (diags?.Length > 0)
                newNode = newNode.WithDiagnosticsGreen(diags);
            var annotations = GetAnnotations();
            if (annotations?.Length > 0)
                newNode = newNode.WithAnnotationsGreen(annotations);
            return newNode;
        }

        return this;
    }

    internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
        => new IdentifierNameSyntax(this.Kind, this.identifier, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new IdentifierNameSyntax(this.Kind, this.identifier, diagnostics: GetDiagnostics(), annotations: annotations);
}

/// <summary>Class which represents the syntax node for qualified name.</summary>
internal sealed partial class QualifiedNameSyntax : NameSyntax
{
    internal readonly NameSyntax left;
    internal readonly SyntaxToken dotToken;
    internal readonly SimpleNameSyntax right;

    internal QualifiedNameSyntax(SyntaxKind kind, NameSyntax left, SyntaxToken dotToken, SimpleNameSyntax right, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        context?.Set(this);
        this.SlotCount = 3;
        this.AdjustFlagsAndWidth(left);
        this.left = left;
        this.AdjustFlagsAndWidth(dotToken);
        this.dotToken = dotToken;
        this.AdjustFlagsAndWidth(right);
        this.right = right;
    }

    /// <summary>NameSyntax node representing the name on the left side of the dot token of the qualified name.</summary>
    public NameSyntax Left => this.left;
    /// <summary>SyntaxToken representing the dot.</summary>
    public SyntaxToken DotToken => this.dotToken;
    /// <summary>SimpleNameSyntax node representing the name on the right side of the dot token of the qualified name.</summary>
    public SimpleNameSyntax Right => this.right;

    internal override GreenNode? GetSlot(int index)
        => index switch
        {
            0 => this.left,
            1 => this.dotToken,
            2 => this.right,
            _ => null,
        };

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.QualifiedNameSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitQualifiedName(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitQualifiedName(this);

    public QualifiedNameSyntax Update(NameSyntax left, SyntaxToken dotToken, SimpleNameSyntax right)
    {
        if (left != this.Left || dotToken != this.DotToken || right != this.Right)
        {
            var newNode = SyntaxFactory.QualifiedName(left, dotToken, right);
            var diags = GetDiagnostics();
            if (diags?.Length > 0)
                newNode = newNode.WithDiagnosticsGreen(diags);
            var annotations = GetAnnotations();
            if (annotations?.Length > 0)
                newNode = newNode.WithAnnotationsGreen(annotations);
            return newNode;
        }

        return this;
    }

    internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
        => new QualifiedNameSyntax(this.Kind, this.left, this.dotToken, this.right, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new QualifiedNameSyntax(this.Kind, this.left, this.dotToken, this.right, diagnostics: GetDiagnostics(), annotations: annotations);
}

/// <summary>Class which represents the syntax node for generic name.</summary>
internal sealed partial class GenericNameSyntax : SimpleNameSyntax
{
    internal readonly SyntaxToken identifier;
    internal readonly TypeArgumentListSyntax typeArgumentList;

    internal GenericNameSyntax(SyntaxKind kind, SyntaxToken identifier, TypeArgumentListSyntax typeArgumentList, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        context?.Set(this);
        this.SlotCount = 2;
        this.AdjustFlagsAndWidth(identifier);
        this.identifier = identifier;
        this.AdjustFlagsAndWidth(typeArgumentList);
        this.typeArgumentList = typeArgumentList;
    }

    /// <summary>SyntaxToken representing the name of the identifier of the generic name.</summary>
    public override SyntaxToken Identifier => this.identifier;
    /// <summary>TypeArgumentListSyntax node representing the list of type arguments of the generic name.</summary>
    public TypeArgumentListSyntax TypeArgumentList => this.typeArgumentList;

    internal override GreenNode? GetSlot(int index)
        => index switch
        {
            0 => this.identifier,
            1 => this.typeArgumentList,
            _ => null,
        };

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.GenericNameSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitGenericName(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitGenericName(this);

    public GenericNameSyntax Update(SyntaxToken identifier, TypeArgumentListSyntax typeArgumentList)
    {
        if (identifier != this.Identifier || typeArgumentList != this.TypeArgumentList)
        {
            var newNode = SyntaxFactory.GenericName(identifier, typeArgumentList);
            var diags = GetDiagnostics();
            if (diags?.Length > 0)
                newNode = newNode.WithDiagnosticsGreen(diags);
            var annotations = GetAnnotations();
            if (annotations?.Length > 0)
                newNode = newNode.WithAnnotationsGreen(annotations);
            return newNode;
        }

        return this;
    }

    internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
        => new GenericNameSyntax(this.Kind, this.identifier, this.typeArgumentList, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new GenericNameSyntax(this.Kind, this.identifier, this.typeArgumentList, diagnostics: GetDiagnostics(), annotations: annotations);
}



/// <summary>Class which represents the syntax node for alias qualified name.</summary>
internal sealed partial class AliasQualifiedNameSyntax : NameSyntax
{
    internal readonly IdentifierNameSyntax alias;
    internal readonly SyntaxToken colonColonToken;
    internal readonly SimpleNameSyntax name;

    internal AliasQualifiedNameSyntax(SyntaxKind kind, IdentifierNameSyntax alias, SyntaxToken colonColonToken, SimpleNameSyntax name, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        context?.Set(this);
        this.SlotCount = 3;
        this.AdjustFlagsAndWidth(alias);
        this.alias = alias;
        this.AdjustFlagsAndWidth(colonColonToken);
        this.colonColonToken = colonColonToken;
        this.AdjustFlagsAndWidth(name);
        this.name = name;
    }

    /// <summary>IdentifierNameSyntax node representing the name of the alias</summary>
    public IdentifierNameSyntax Alias => this.alias;
    /// <summary>SyntaxToken representing colon colon.</summary>
    public SyntaxToken ColonColonToken => this.colonColonToken;
    /// <summary>SimpleNameSyntax node representing the name that is being alias qualified.</summary>
    public SimpleNameSyntax Name => this.name;

    internal override GreenNode? GetSlot(int index)
        => index switch
        {
            0 => this.alias,
            1 => this.colonColonToken,
            2 => this.name,
            _ => null,
        };

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.AliasQualifiedNameSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitAliasQualifiedName(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitAliasQualifiedName(this);

    public AliasQualifiedNameSyntax Update(IdentifierNameSyntax alias, SyntaxToken colonColonToken, SimpleNameSyntax name)
    {
        if (alias != this.Alias || colonColonToken != this.ColonColonToken || name != this.Name)
        {
            var newNode = SyntaxFactory.AliasQualifiedName(alias, colonColonToken, name);
            var diags = GetDiagnostics();
            if (diags?.Length > 0)
                newNode = newNode.WithDiagnosticsGreen(diags);
            var annotations = GetAnnotations();
            if (annotations?.Length > 0)
                newNode = newNode.WithAnnotationsGreen(annotations);
            return newNode;
        }

        return this;
    }

    internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
        => new AliasQualifiedNameSyntax(this.Kind, this.alias, this.colonColonToken, this.name, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new AliasQualifiedNameSyntax(this.Kind, this.alias, this.colonColonToken, this.name, diagnostics: GetDiagnostics(), annotations: annotations);
}
