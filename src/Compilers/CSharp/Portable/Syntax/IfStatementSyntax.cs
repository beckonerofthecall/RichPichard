// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax
{/// <summary>
 /// Represents an if statement syntax.
 /// </summary>
 /// <remarks>
 /// <para>This node is associated with the following syntax kinds:</para>
 /// <list type="bullet">
 /// <item><description><see cref="SyntaxKind.IfStatement"/></description></item>
 /// </list>
 /// </remarks>
    public sealed partial class IfStatementSyntax : StatementSyntax
    {
        private SyntaxNode? attributeLists;
        private ExpressionSyntax? condition;
        private StatementSyntax? statement;
        private ElseClauseSyntax? @else;

        internal IfStatementSyntax(InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
          : base(green, parent, position)
        {
        }

        public override SyntaxList<AttributeListSyntax> AttributeLists => new SyntaxList<AttributeListSyntax>(GetRed(ref this.attributeLists, 0));

        /// <summary>
        /// Gets a SyntaxToken that represents the if keyword.
        /// </summary>
        public SyntaxToken IfKeyword => new SyntaxToken(this, ((InternalSyntax.IfStatementSyntax)this.Green).ifKeyword, GetChildPosition(1), GetChildIndex(1));

        /// <summary>
        /// Gets a SyntaxToken that represents the open parenthesis before the if statement's condition expression.
        /// </summary>
        public SyntaxToken OpenParenToken => new SyntaxToken(this, ((InternalSyntax.IfStatementSyntax)this.Green).openParenToken, GetChildPosition(2), GetChildIndex(2));

        /// <summary>
        /// Gets an ExpressionSyntax that represents the condition of the if statement.
        /// </summary>
        public ExpressionSyntax Condition => GetRed(ref this.condition, 3)!;

        /// <summary>
        /// Gets a SyntaxToken that represents the close parenthesis after the if statement's condition expression.
        /// </summary>
        public SyntaxToken CloseParenToken => new SyntaxToken(this, ((InternalSyntax.IfStatementSyntax)this.Green).closeParenToken, GetChildPosition(4), GetChildIndex(4));

        /// <summary>
        /// Gets a StatementSyntax the represents the statement to be executed when the condition is true.
        /// </summary>
        public StatementSyntax Statement => GetRed(ref this.statement, 5)!;

        /// <summary>
        /// Gets an ElseClauseSyntax that represents the statement to be executed when the condition is false if such statement exists.
        /// </summary>
        public ElseClauseSyntax? Else => GetRed(ref this.@else, 6);

        internal override SyntaxNode? GetNodeSlot(int index)
            => index switch
            {
                0 => GetRedAtZero(ref this.attributeLists)!,
                3 => GetRed(ref this.condition, 3)!,
                5 => GetRed(ref this.statement, 5)!,
                6 => GetRed(ref this.@else, 6),
                _ => null,
            };

        internal override SyntaxNode? GetCachedSlot(int index)
            => index switch
            {
                0 => this.attributeLists,
                3 => this.condition,
                5 => this.statement,
                6 => this.@else,
                _ => null,
            };

        public override void Accept(CSharpSyntaxVisitor visitor) => visitor.VisitIfStatement(this);
        public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default => visitor.VisitIfStatement(this);

        public IfStatementSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax? @else)
        {
            if (attributeLists != this.AttributeLists || ifKeyword != this.IfKeyword || openParenToken != this.OpenParenToken || condition != this.Condition || closeParenToken != this.CloseParenToken || statement != this.Statement || @else != this.Else)
            {
                var newNode = SyntaxFactory.IfStatement(attributeLists, ifKeyword, openParenToken, condition, closeParenToken, statement, @else);
                var annotations = GetAnnotations();
                return annotations?.Length > 0 ? newNode.WithAnnotations(annotations) : newNode;
            }

            return this;
        }

        public IfStatementSyntax Update(SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax? @else)
            => Update(AttributeLists, ifKeyword, openParenToken, condition, closeParenToken, statement, @else);

        internal override StatementSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists) => WithAttributeLists(attributeLists);
        public new IfStatementSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists) => Update(attributeLists, this.IfKeyword, this.OpenParenToken, this.Condition, this.CloseParenToken, this.Statement, this.Else);
        public IfStatementSyntax WithIfKeyword(SyntaxToken ifKeyword) => Update(this.AttributeLists, ifKeyword, this.OpenParenToken, this.Condition, this.CloseParenToken, this.Statement, this.Else);
        public IfStatementSyntax WithOpenParenToken(SyntaxToken openParenToken) => Update(this.AttributeLists, this.IfKeyword, openParenToken, this.Condition, this.CloseParenToken, this.Statement, this.Else);
        public IfStatementSyntax WithCondition(ExpressionSyntax condition) => Update(this.AttributeLists, this.IfKeyword, this.OpenParenToken, condition, this.CloseParenToken, this.Statement, this.Else);
        public IfStatementSyntax WithCloseParenToken(SyntaxToken closeParenToken) => Update(this.AttributeLists, this.IfKeyword, this.OpenParenToken, this.Condition, closeParenToken, this.Statement, this.Else);
        public IfStatementSyntax WithStatement(StatementSyntax statement) => Update(this.AttributeLists, this.IfKeyword, this.OpenParenToken, this.Condition, this.CloseParenToken, statement, this.Else);
        public IfStatementSyntax WithElse(ElseClauseSyntax? @else) => Update(this.AttributeLists, this.IfKeyword, this.OpenParenToken, this.Condition, this.CloseParenToken, this.Statement, @else);

        internal override StatementSyntax AddAttributeListsCore(params AttributeListSyntax[] items) => AddAttributeLists(items);
        public new IfStatementSyntax AddAttributeLists(params AttributeListSyntax[] items) => WithAttributeLists(this.AttributeLists.AddRange(items));
    }
}

namespace Microsoft.CodeAnalysis.CSharp
{
    public partial class SyntaxFactory
    {/// <summary>Creates a new IfStatementSyntax instance.</summary>
        public static IfStatementSyntax IfStatement(SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax? @else)
        {
            return (IfStatementSyntax)Syntax.InternalSyntax.SyntaxFactory.IfStatement(attributeLists.Node.ToGreenList<Syntax.InternalSyntax.AttributeListSyntax>(), (Syntax.InternalSyntax.SyntaxToken)ifKeyword.Node!, (Syntax.InternalSyntax.SyntaxToken)openParenToken.Node!, (Syntax.InternalSyntax.ExpressionSyntax)condition.Green, (Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node!, (Syntax.InternalSyntax.StatementSyntax)statement.Green, @else == null ? null : (Syntax.InternalSyntax.ElseClauseSyntax)@else.Green).CreateRed();
        }

        /// <summary>Creates a new IfStatementSyntax instance.</summary>
        public static IfStatementSyntax IfStatement(SyntaxList<AttributeListSyntax> attributeLists, ExpressionSyntax condition, StatementSyntax statement, ElseClauseSyntax? @else)
            => IfStatement(attributeLists, Token(SyntaxKind.IfKeyword), Token(SyntaxKind.OpenParenToken), condition, Token(SyntaxKind.CloseParenToken), statement, @else);

        /// <summary>Creates a new IfStatementSyntax instance.</summary>
        public static IfStatementSyntax IfStatement(ExpressionSyntax condition, StatementSyntax statement)
            => IfStatement(default, Token(SyntaxKind.IfKeyword), Token(SyntaxKind.OpenParenToken), condition, Token(SyntaxKind.CloseParenToken), statement, default);

        public static IfStatementSyntax IfStatement(ExpressionSyntax condition, StatementSyntax statement, ElseClauseSyntax? @else)
            => IfStatement(attributeLists: default, condition, statement, @else);

        public static IfStatementSyntax IfStatement(SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax? @else)
            => IfStatement(attributeLists: default, ifKeyword, openParenToken, condition, closeParenToken, statement, @else);

        /// <summary>Creates a new ElseClauseSyntax instance.</summary>
        public static ElseClauseSyntax ElseClause(SyntaxToken elseKeyword, StatementSyntax statement)
        {
            if (elseKeyword.Kind() != SyntaxKind.ElseKeyword) throw new ArgumentException(nameof(elseKeyword));
            if (statement == null) throw new ArgumentNullException(nameof(statement));
            return (ElseClauseSyntax)Syntax.InternalSyntax.SyntaxFactory.ElseClause((Syntax.InternalSyntax.SyntaxToken)elseKeyword.Node!, (Syntax.InternalSyntax.StatementSyntax)statement.Green).CreateRed();
        }

        /// <summary>Creates a new ElseClauseSyntax instance.</summary>
        public static ElseClauseSyntax ElseClause(StatementSyntax statement)
            => ElseClause(Token(SyntaxKind.ElseKeyword), statement);
    }
}
