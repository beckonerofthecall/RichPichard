// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CoreSyntax = Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

/// <summary>Provides the base class from which the classes that represent type syntax nodes are derived. This is an abstract class.</summary>
internal abstract partial class TypeSyntax : ExpressionSyntax
{
    internal TypeSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    { }

    public bool IsVar => IsIdentifierName("var");
    public bool IsUnmanaged => IsIdentifierName("unmanaged");
    public bool IsNotNull => IsIdentifierName("notnull");
    public bool IsNint => IsIdentifierName("nint");
    public bool IsNuint => IsIdentifierName("nuint");

    private bool IsIdentifierName(string id) => this is IdentifierNameSyntax name && name.Identifier.ToString() == id;

    public bool IsRef => Kind == SyntaxKind.RefType;
}

/// <summary>Class which represents the syntax node for predefined types.</summary>
internal sealed partial class PredefinedTypeSyntax : TypeSyntax
{
    internal readonly SyntaxToken keyword;

    internal PredefinedTypeSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        this.SetFactoryContext(context);
        this.SlotCount = 1;
        this.AdjustFlagsAndWidth(keyword);
        this.keyword = keyword;
    }

    /// <summary>SyntaxToken which represents the keyword corresponding to the predefined type.</summary>
    public SyntaxToken Keyword => this.keyword;

    internal override GreenNode? GetSlot(int index)
        => index == 0 ? this.keyword : null;

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.PredefinedTypeSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitPredefinedType(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitPredefinedType(this);

    public PredefinedTypeSyntax Update(SyntaxToken keyword)
    {
        if (keyword != this.Keyword)
        {
            var newNode = SyntaxFactory.PredefinedType(keyword);
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
        => new PredefinedTypeSyntax(this.Kind, this.keyword, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new PredefinedTypeSyntax(this.Kind, this.keyword, diagnostics: GetDiagnostics(), annotations: annotations);
}

/// <summary>Class which represents the syntax node for the array type.</summary>
internal sealed partial class ArrayTypeSyntax : TypeSyntax
{
    internal readonly TypeSyntax elementType;
    internal readonly GreenNode? rankSpecifiers;

    internal ArrayTypeSyntax(SyntaxKind kind, TypeSyntax elementType, GreenNode? rankSpecifiers, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        this.SetFactoryContext(context);
        this.SlotCount = 2;
        this.AdjustFlagsAndWidth(elementType);
        this.elementType = elementType;
        if (rankSpecifiers != null)
        {
            this.AdjustFlagsAndWidth(rankSpecifiers);
            this.rankSpecifiers = rankSpecifiers;
        }
    }

    /// <summary>TypeSyntax node representing the type of the element of the array.</summary>
    public TypeSyntax ElementType => this.elementType;
    /// <summary>SyntaxList of ArrayRankSpecifierSyntax nodes representing the list of rank specifiers for the array.</summary>
    public CoreSyntax.SyntaxList<ArrayRankSpecifierSyntax> RankSpecifiers => new CoreSyntax.SyntaxList<ArrayRankSpecifierSyntax>(this.rankSpecifiers);

    internal override GreenNode? GetSlot(int index)
        => index switch
        {
            0 => this.elementType,
            1 => this.rankSpecifiers,
            _ => null,
        };

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.ArrayTypeSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitArrayType(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitArrayType(this);

    public ArrayTypeSyntax Update(TypeSyntax elementType, CoreSyntax.SyntaxList<ArrayRankSpecifierSyntax> rankSpecifiers)
    {
        if (elementType != this.ElementType || rankSpecifiers != this.RankSpecifiers)
        {
            var newNode = SyntaxFactory.ArrayType(elementType, rankSpecifiers);
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
        => new ArrayTypeSyntax(this.Kind, this.elementType, this.rankSpecifiers, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new ArrayTypeSyntax(this.Kind, this.elementType, this.rankSpecifiers, diagnostics: GetDiagnostics(), annotations: annotations);
}



/// <summary>Class which represents the syntax node for pointer type.</summary>
internal sealed partial class PointerTypeSyntax : TypeSyntax
{
    internal readonly TypeSyntax elementType;
    internal readonly SyntaxToken asteriskToken;

    internal PointerTypeSyntax(SyntaxKind kind, TypeSyntax elementType, SyntaxToken asteriskToken, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        this.SetFactoryContext(context);
        this.SlotCount = 2;
        this.AdjustFlagsAndWidth(elementType);
        this.elementType = elementType;
        this.AdjustFlagsAndWidth(asteriskToken);
        this.asteriskToken = asteriskToken;
    }

    /// <summary>TypeSyntax node that represents the element type of the pointer.</summary>
    public TypeSyntax ElementType => this.elementType;
    /// <summary>SyntaxToken representing the asterisk.</summary>
    public SyntaxToken AsteriskToken => this.asteriskToken;

    internal override GreenNode? GetSlot(int index)
        => index switch
        {
            0 => this.elementType,
            1 => this.asteriskToken,
            _ => null,
        };

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.PointerTypeSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitPointerType(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitPointerType(this);

    public PointerTypeSyntax Update(TypeSyntax elementType, SyntaxToken asteriskToken)
    {
        if (elementType != this.ElementType || asteriskToken != this.AsteriskToken)
        {
            var newNode = SyntaxFactory.PointerType(elementType, asteriskToken);
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
        => new PointerTypeSyntax(this.Kind, this.elementType, this.asteriskToken, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new PointerTypeSyntax(this.Kind, this.elementType, this.asteriskToken, diagnostics: GetDiagnostics(), annotations: annotations);
}

internal sealed partial class FunctionPointerTypeSyntax : TypeSyntax
{
    internal readonly SyntaxToken delegateKeyword;
    internal readonly SyntaxToken asteriskToken;
    internal readonly FunctionPointerCallingConventionSyntax? callingConvention;
    internal readonly FunctionPointerParameterListSyntax parameterList;

    internal FunctionPointerTypeSyntax(SyntaxKind kind, SyntaxToken delegateKeyword, SyntaxToken asteriskToken, FunctionPointerCallingConventionSyntax? callingConvention, FunctionPointerParameterListSyntax parameterList, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        this.SetFactoryContext(context);
        this.SlotCount = 4;
        this.AdjustFlagsAndWidth(delegateKeyword);
        this.delegateKeyword = delegateKeyword;
        this.AdjustFlagsAndWidth(asteriskToken);
        this.asteriskToken = asteriskToken;
        if (callingConvention != null)
        {
            this.AdjustFlagsAndWidth(callingConvention);
            this.callingConvention = callingConvention;
        }
        this.AdjustFlagsAndWidth(parameterList);
        this.parameterList = parameterList;
    }

    /// <summary>SyntaxToken representing the delegate keyword.</summary>
    public SyntaxToken DelegateKeyword => this.delegateKeyword;
    /// <summary>SyntaxToken representing the asterisk.</summary>
    public SyntaxToken AsteriskToken => this.asteriskToken;
    /// <summary>Node representing the optional calling convention.</summary>
    public FunctionPointerCallingConventionSyntax? CallingConvention => this.callingConvention;
    /// <summary>List of the parameter types and return type of the function pointer.</summary>
    public FunctionPointerParameterListSyntax ParameterList => this.parameterList;

    internal override GreenNode? GetSlot(int index)
        => index switch
        {
            0 => this.delegateKeyword,
            1 => this.asteriskToken,
            2 => this.callingConvention,
            3 => this.parameterList,
            _ => null,
        };

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.FunctionPointerTypeSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitFunctionPointerType(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitFunctionPointerType(this);

    public FunctionPointerTypeSyntax Update(SyntaxToken delegateKeyword, SyntaxToken asteriskToken, FunctionPointerCallingConventionSyntax callingConvention, FunctionPointerParameterListSyntax parameterList)
    {
        if (delegateKeyword != this.DelegateKeyword || asteriskToken != this.AsteriskToken || callingConvention != this.CallingConvention || parameterList != this.ParameterList)
        {
            var newNode = SyntaxFactory.FunctionPointerType(delegateKeyword, asteriskToken, callingConvention, parameterList);
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
        => new FunctionPointerTypeSyntax(this.Kind, this.delegateKeyword, this.asteriskToken, this.callingConvention, this.parameterList, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new FunctionPointerTypeSyntax(this.Kind, this.delegateKeyword, this.asteriskToken, this.callingConvention, this.parameterList, diagnostics: GetDiagnostics(), annotations: annotations);
}



/// <summary>Class which represents the syntax node for a nullable type.</summary>
internal sealed partial class NullableTypeSyntax : TypeSyntax
{
    internal readonly TypeSyntax elementType;
    internal readonly SyntaxToken questionToken;

    internal NullableTypeSyntax(SyntaxKind kind, TypeSyntax elementType, SyntaxToken questionToken, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        this.SetFactoryContext(context);
        this.SlotCount = 2;
        this.AdjustFlagsAndWidth(elementType);
        this.elementType = elementType;
        this.AdjustFlagsAndWidth(questionToken);
        this.questionToken = questionToken;
    }

    /// <summary>TypeSyntax node representing the type of the element.</summary>
    public TypeSyntax ElementType => this.elementType;
    /// <summary>SyntaxToken representing the question mark.</summary>
    public SyntaxToken QuestionToken => this.questionToken;

    internal override GreenNode? GetSlot(int index)
        => index switch
        {
            0 => this.elementType,
            1 => this.questionToken,
            _ => null,
        };

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.NullableTypeSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitNullableType(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitNullableType(this);

    public NullableTypeSyntax Update(TypeSyntax elementType, SyntaxToken questionToken)
    {
        if (elementType != this.ElementType || questionToken != this.QuestionToken)
        {
            var newNode = SyntaxFactory.NullableType(elementType, questionToken);
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
        => new NullableTypeSyntax(this.Kind, this.elementType, this.questionToken, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new NullableTypeSyntax(this.Kind, this.elementType, this.questionToken, diagnostics: GetDiagnostics(), annotations: annotations);
}

/// <summary>Class which represents the syntax node for tuple type.</summary>
internal sealed partial class TupleTypeSyntax : TypeSyntax
{
    internal readonly SyntaxToken openParenToken;
    internal readonly GreenNode? elements;
    internal readonly SyntaxToken closeParenToken;

    internal TupleTypeSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode? elements, SyntaxToken closeParenToken, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        this.SetFactoryContext(context);
        this.SlotCount = 3;
        this.AdjustFlagsAndWidth(openParenToken);
        this.openParenToken = openParenToken;
        if (elements != null)
        {
            this.AdjustFlagsAndWidth(elements);
            this.elements = elements;
        }
        this.AdjustFlagsAndWidth(closeParenToken);
        this.closeParenToken = closeParenToken;
    }

    /// <summary>SyntaxToken representing the open parenthesis.</summary>
    public SyntaxToken OpenParenToken => this.openParenToken;
    public CoreSyntax.SeparatedSyntaxList<TupleElementSyntax> Elements => new CoreSyntax.SeparatedSyntaxList<TupleElementSyntax>(new CoreSyntax.SyntaxList<CSharpSyntaxNode>(this.elements));
    /// <summary>SyntaxToken representing the close parenthesis.</summary>
    public SyntaxToken CloseParenToken => this.closeParenToken;

    internal override GreenNode? GetSlot(int index)
        => index switch
        {
            0 => this.openParenToken,
            1 => this.elements,
            2 => this.closeParenToken,
            _ => null,
        };

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.TupleTypeSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitTupleType(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitTupleType(this);

    public TupleTypeSyntax Update(SyntaxToken openParenToken, CoreSyntax.SeparatedSyntaxList<TupleElementSyntax> elements, SyntaxToken closeParenToken)
    {
        if (openParenToken != this.OpenParenToken || elements != this.Elements || closeParenToken != this.CloseParenToken)
        {
            var newNode = SyntaxFactory.TupleType(openParenToken, elements, closeParenToken);
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
        => new TupleTypeSyntax(this.Kind, this.openParenToken, this.elements, this.closeParenToken, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new TupleTypeSyntax(this.Kind, this.openParenToken, this.elements, this.closeParenToken, diagnostics: GetDiagnostics(), annotations: annotations);
}

/// <summary>Class which represents a placeholder in the type argument list of an unbound generic type.</summary>
internal sealed partial class OmittedTypeArgumentSyntax : TypeSyntax
{
    internal readonly SyntaxToken omittedTypeArgumentToken;

    internal OmittedTypeArgumentSyntax(SyntaxKind kind, SyntaxToken omittedTypeArgumentToken, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        this.SetFactoryContext(context);
        this.SlotCount = 1;
        this.AdjustFlagsAndWidth(omittedTypeArgumentToken);
        this.omittedTypeArgumentToken = omittedTypeArgumentToken;
    }

    /// <summary>SyntaxToken representing the omitted type argument.</summary>
    public SyntaxToken OmittedTypeArgumentToken => this.omittedTypeArgumentToken;

    internal override GreenNode? GetSlot(int index)
        => index == 0 ? this.omittedTypeArgumentToken : null;

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.OmittedTypeArgumentSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitOmittedTypeArgument(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitOmittedTypeArgument(this);

    public OmittedTypeArgumentSyntax Update(SyntaxToken omittedTypeArgumentToken)
    {
        if (omittedTypeArgumentToken != this.OmittedTypeArgumentToken)
        {
            var newNode = SyntaxFactory.OmittedTypeArgument(omittedTypeArgumentToken);
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
        => new OmittedTypeArgumentSyntax(this.Kind, this.omittedTypeArgumentToken, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new OmittedTypeArgumentSyntax(this.Kind, this.omittedTypeArgumentToken, diagnostics: GetDiagnostics(), annotations: annotations);
}


/// <summary>The ref modifier of a method's return value or a local.</summary>
internal sealed partial class RefTypeSyntax : TypeSyntax
{
    internal readonly SyntaxToken refKeyword;
    internal readonly SyntaxToken? readOnlyKeyword;
    internal readonly TypeSyntax type;

    internal RefTypeSyntax(SyntaxKind kind, SyntaxToken refKeyword, SyntaxToken? readOnlyKeyword, TypeSyntax type, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        this.SetFactoryContext(context);
        this.SlotCount = 3;
        this.AdjustFlagsAndWidth(refKeyword);
        this.refKeyword = refKeyword;
        if (readOnlyKeyword != null)
        {
            this.AdjustFlagsAndWidth(readOnlyKeyword);
            this.readOnlyKeyword = readOnlyKeyword;
        }
        this.AdjustFlagsAndWidth(type);
        this.type = type;
    }

    public SyntaxToken RefKeyword => this.refKeyword;
    /// <summary>Gets the optional "readonly" keyword.</summary>
    public SyntaxToken? ReadOnlyKeyword => this.readOnlyKeyword;
    public TypeSyntax Type => this.type;

    internal override GreenNode? GetSlot(int index)
        => index switch
        {
            0 => this.refKeyword,
            1 => this.readOnlyKeyword,
            2 => this.type,
            _ => null,
        };

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.RefTypeSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitRefType(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitRefType(this);

    public RefTypeSyntax Update(SyntaxToken refKeyword, SyntaxToken readOnlyKeyword, TypeSyntax type)
    {
        if (refKeyword != this.RefKeyword || readOnlyKeyword != this.ReadOnlyKeyword || type != this.Type)
        {
            var newNode = SyntaxFactory.RefType(refKeyword, readOnlyKeyword, type);
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
        => new RefTypeSyntax(this.Kind, this.refKeyword, this.readOnlyKeyword, this.type, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new RefTypeSyntax(this.Kind, this.refKeyword, this.readOnlyKeyword, this.type, diagnostics: GetDiagnostics(), annotations: annotations);
}

/// <summary>The 'scoped' modifier of a local.</summary>
internal sealed partial class ScopedTypeSyntax : TypeSyntax
{
    internal readonly SyntaxToken scopedKeyword;
    internal readonly TypeSyntax type;

    internal ScopedTypeSyntax(SyntaxKind kind, SyntaxToken scopedKeyword, TypeSyntax type, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        this.SetFactoryContext(context);
        this.SlotCount = 2;
        this.AdjustFlagsAndWidth(scopedKeyword);
        this.scopedKeyword = scopedKeyword;
        this.AdjustFlagsAndWidth(type);
        this.type = type;
    }

    public SyntaxToken ScopedKeyword => this.scopedKeyword;
    public TypeSyntax Type => this.type;

    internal override GreenNode? GetSlot(int index)
        => index switch
        {
            0 => this.scopedKeyword,
            1 => this.type,
            _ => null,
        };

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.ScopedTypeSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitScopedType(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitScopedType(this);

    public ScopedTypeSyntax Update(SyntaxToken scopedKeyword, TypeSyntax type)
    {
        if (scopedKeyword != this.ScopedKeyword || type != this.Type)
        {
            var newNode = SyntaxFactory.ScopedType(scopedKeyword, type);
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
        => new ScopedTypeSyntax(this.Kind, this.scopedKeyword, this.type, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new ScopedTypeSyntax(this.Kind, this.scopedKeyword, this.type, diagnostics: GetDiagnostics(), annotations: annotations);
}
