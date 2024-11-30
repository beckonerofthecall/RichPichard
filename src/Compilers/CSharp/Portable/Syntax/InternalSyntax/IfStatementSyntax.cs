// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

using System;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using CoreSyntax = CodeAnalysis.Syntax.InternalSyntax;

/// <summary>
/// Represents an if statement syntax.
/// </summary>
internal sealed partial class IfStatementSyntax : StatementSyntax
{
    internal readonly GreenNode? attributeLists;
    internal readonly SyntaxToken ifKeyword;
    internal readonly SyntaxToken openParenToken;
    internal readonly ExpressionSyntax condition;
    internal readonly SyntaxToken closeParenToken;
    internal readonly StatementSyntax statement;
    internal readonly ElseClauseSyntax? @else;

    internal IfStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax? @else, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        context?.Set(this);
        this.SlotCount = 7;
        if (attributeLists != null)
        {
            this.AdjustFlagsAndWidth(attributeLists);
            this.attributeLists = attributeLists;
        }
        this.AdjustFlagsAndWidth(ifKeyword);
        this.ifKeyword = ifKeyword;
        this.AdjustFlagsAndWidth(openParenToken);
        this.openParenToken = openParenToken;
        this.AdjustFlagsAndWidth(condition);
        this.condition = condition;
        this.AdjustFlagsAndWidth(closeParenToken);
        this.closeParenToken = closeParenToken;
        this.AdjustFlagsAndWidth(statement);
        this.statement = statement;
        if (@else != null)
        {
            this.AdjustFlagsAndWidth(@else);
            this.@else = @else;
        }
    }

    public override CoreSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new CoreSyntax.SyntaxList<AttributeListSyntax>(this.attributeLists);
    /// <summary>
    /// Gets a SyntaxToken that represents the if keyword.
    /// </summary>
    public SyntaxToken IfKeyword => this.ifKeyword;
    /// <summary>
    /// Gets a SyntaxToken that represents the open parenthesis before the if statement's condition expression.
    /// </summary>
    public SyntaxToken OpenParenToken => this.openParenToken;
    /// <summary>
    /// Gets an ExpressionSyntax that represents the condition of the if statement.
    /// </summary>
    public ExpressionSyntax Condition => this.condition;
    /// <summary>
    /// Gets a SyntaxToken that represents the close parenthesis after the if statement's condition expression.
    /// </summary>
    public SyntaxToken CloseParenToken => this.closeParenToken;
    /// <summary>
    /// Gets a StatementSyntax the represents the statement to be executed when the condition is true.
    /// </summary>
    public StatementSyntax Statement => this.statement;
    /// <summary>
    /// Gets an ElseClauseSyntax that represents the statement to be executed when the condition is false if such statement exists.
    /// </summary>
    public ElseClauseSyntax? Else => this.@else;

    internal override GreenNode? GetSlot(int index)
        => index switch
        {
            0 => this.attributeLists,
            1 => this.ifKeyword,
            2 => this.openParenToken,
            3 => this.condition,
            4 => this.closeParenToken,
            5 => this.statement,
            6 => this.@else,
            _ => null,
        };

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.IfStatementSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitIfStatement(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitIfStatement(this);

    public IfStatementSyntax Update(CoreSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax @else)
    {
        if (attributeLists != this.AttributeLists || ifKeyword != this.IfKeyword || openParenToken != this.OpenParenToken || condition != this.Condition || closeParenToken != this.CloseParenToken || statement != this.Statement || @else != this.Else)
        {
            var newNode = SyntaxFactory.IfStatement(attributeLists, ifKeyword, openParenToken, condition, closeParenToken, statement, @else);
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
        => new IfStatementSyntax(this.Kind, this.attributeLists, this.ifKeyword, this.openParenToken, this.condition, this.closeParenToken, this.statement, this.@else, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new IfStatementSyntax(this.Kind, this.attributeLists, this.ifKeyword, this.openParenToken, this.condition, this.closeParenToken, this.statement, this.@else, diagnostics: GetDiagnostics(), annotations: annotations);
}

/// <summary>Represents an else statement syntax.</summary>
internal sealed partial class ElseClauseSyntax : CSharpSyntaxNode
{
    internal readonly SyntaxToken elseKeyword;
    internal readonly StatementSyntax statement;

    internal ElseClauseSyntax(SyntaxKind kind, SyntaxToken elseKeyword, StatementSyntax statement, SyntaxFactoryContext context = null, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
      : base(kind, diagnostics, annotations)
    {
        context?.Set(this);
        this.SlotCount = 2;
        this.AdjustFlagsAndWidth(elseKeyword);
        this.elseKeyword = elseKeyword;
        this.AdjustFlagsAndWidth(statement);
        this.statement = statement;
    }

    /// <summary>
    /// Gets a syntax token
    /// </summary>
    public SyntaxToken ElseKeyword => this.elseKeyword;
    public StatementSyntax Statement => this.statement;

    internal override GreenNode? GetSlot(int index)
        => index switch
        {
            0 => this.elseKeyword,
            1 => this.statement,
            _ => null,
        };

    internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => new CSharp.Syntax.ElseClauseSyntax(this, parent, position);

    public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitElseClause(this);
    public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) => visitor.VisitElseClause(this);

    public ElseClauseSyntax Update(SyntaxToken elseKeyword, StatementSyntax statement)
    {
        if (elseKeyword != this.ElseKeyword || statement != this.Statement)
        {
            var newNode = SyntaxFactory.ElseClause(elseKeyword, statement);
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
        => new ElseClauseSyntax(this.Kind, this.elseKeyword, this.statement, diagnostics: diagnostics, annotations: GetAnnotations());

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        => new ElseClauseSyntax(this.Kind, this.elseKeyword, this.statement, diagnostics: GetDiagnostics(), annotations: annotations);
}

internal static partial class SyntaxFactory
{
    public static IfStatementSyntax IfStatement(SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax? @else)
    {
#if DEBUG
        if (ifKeyword == null) throw new ArgumentNullException(nameof(ifKeyword));
        if (ifKeyword.Kind != SyntaxKind.IfKeyword) throw new ArgumentException(nameof(ifKeyword));
        if (openParenToken == null) throw new ArgumentNullException(nameof(openParenToken));
        if (openParenToken.Kind != SyntaxKind.OpenParenToken) throw new ArgumentException(nameof(openParenToken));
        if (condition == null) throw new ArgumentNullException(nameof(condition));
        if (closeParenToken == null) throw new ArgumentNullException(nameof(closeParenToken));
        if (closeParenToken.Kind != SyntaxKind.CloseParenToken) throw new ArgumentException(nameof(closeParenToken));
        if (statement == null) throw new ArgumentNullException(nameof(statement));
#endif

        return new IfStatementSyntax(SyntaxKind.IfStatement, attributeLists.Node, ifKeyword, openParenToken, condition, closeParenToken, statement, @else);
    }

    public static ElseClauseSyntax ElseClause(SyntaxToken elseKeyword, StatementSyntax statement)
    {
#if DEBUG
        if (elseKeyword == null) throw new ArgumentNullException(nameof(elseKeyword));
        if (elseKeyword.Kind != SyntaxKind.ElseKeyword) throw new ArgumentException(nameof(elseKeyword));
        if (statement == null) throw new ArgumentNullException(nameof(statement));
#endif

        int hash;
        var cached = SyntaxNodeCache.TryGetNode((int)SyntaxKind.ElseClause, elseKeyword, statement, out hash);
        if (cached != null) return (ElseClauseSyntax)cached;

        var result = new ElseClauseSyntax(SyntaxKind.ElseClause, elseKeyword, statement);
        if (hash >= 0)
        {
            SyntaxNodeCache.AddNode(result, hash);
        }

        return result;
    }
}
