﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp
{
    internal sealed partial class LocalRewriter
    {
        /// <summary>
        /// If the condition has a constant value, then just use the selected branch.
        /// e.g. "true ? x : y" becomes "x".
        /// </summary>
        public override BoundNode VisitConditionalOperator(BoundConditionalOperator node)
        {
            // just a fact, not a requirement (VisitExpression would have rewritten otherwise)
            Debug.Assert(node.ConstantValueOpt == null);

            var rewrittenCondition = VisitExpression(node.Condition);
            var rewrittenConsequence = VisitExpression(node.Consequence);
            var rewrittenAlternative = VisitExpression(node.Alternative);

            if (rewrittenCondition.ConstantValueOpt == null)
            {
                return node.Update(node.IsRef, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, node.ConstantValueOpt, node.NaturalTypeOpt, node.WasTargetTyped, node.Type);
            }

            return RewriteConditionalOperator(
                node.Syntax,
                rewrittenCondition,
                rewrittenConsequence,
                rewrittenAlternative,
                node.ConstantValueOpt,
                node.Type,
                node.IsRef);
        }

        private static BoundExpression RewriteConditionalOperator(
            SyntaxNode syntax,
            BoundExpression rewrittenCondition,
            BoundExpression rewrittenConsequence,
            BoundExpression rewrittenAlternative,
            ConstantValue? constantValueOpt,
            TypeSymbol rewrittenType,
            bool isRef)
        {
            ConstantValue? conditionConstantValue = rewrittenCondition.ConstantValueOpt;
            if (conditionConstantValue == ConstantValue.True)
            {
                return rewrittenConsequence;
            }
            else if (conditionConstantValue == ConstantValue.False)
            {
                return rewrittenAlternative;
            }
            else
            {
                return new BoundConditionalOperator(
                    syntax,
                    isRef,
                    rewrittenCondition,
                    rewrittenConsequence,
                    rewrittenAlternative,
                    constantValueOpt,
                    rewrittenType,
                    wasTargetTyped: false,
                    rewrittenType);
            }
        }
    }
}
