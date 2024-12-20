﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp
{
    /// <summary>
    /// A region analysis walker that computes the set of variables whose values flow into (are used
    /// in) the region. A variable assigned outside is used inside if an analysis that leaves the
    /// variable unassigned on entry to the region would cause the generation of "unassigned" errors
    /// within the region.
    /// </summary>
    internal class DataFlowsInWalker : AbstractRegionDataFlowPass
    {
        // TODO: normalize the result by removing variables that are unassigned in an unmodified
        // flow analysis.
        private readonly HashSet<Symbol> _dataFlowsIn = new HashSet<Symbol>();

        private DataFlowsInWalker(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion,
            HashSet<Symbol> unassignedVariables, HashSet<PrefixUnaryExpressionSyntax> unassignedVariableAddressOfSyntaxes)
            : base(compilation, member, node, firstInRegion, lastInRegion, unassignedVariables, unassignedVariableAddressOfSyntaxes)
        {
        }

        internal static HashSet<Symbol> Analyze(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion,
            HashSet<Symbol> unassignedVariables, HashSet<PrefixUnaryExpressionSyntax> unassignedVariableAddressOfSyntaxes, out bool? succeeded)
        {
            var walker = new DataFlowsInWalker(compilation, member, node, firstInRegion, lastInRegion, unassignedVariables, unassignedVariableAddressOfSyntaxes);
            try
            {
                bool badRegion = false;
                var result = walker.Analyze(ref badRegion);
                succeeded = !badRegion;
                return badRegion ? new HashSet<Symbol>() : result;
            }
            finally
            {
                walker.Free();
            }
        }

        private HashSet<Symbol> Analyze(ref bool badRegion)
        {
            base.Analyze(ref badRegion, null);
            return _dataFlowsIn;
        }

        protected override LocalState TopState()
        {
            return new LocalState(BitVector.Empty);
        }

        private LocalState ResetState(LocalState state)
        {
            bool unreachable = !state.Reachable;
            state = TopState();
            if (unreachable)
            {
                state.Assign(0);
            }
            return state;
        }

        protected override void EnterRegion()
        {
            this.State = ResetState(this.State);
            _dataFlowsIn.Clear();
            base.EnterRegion();
        }

        protected override void NoteBranch(
            PendingBranch pending,
            BoundNode gotoStmt,
            BoundStatement targetStmt)
        {
            targetStmt.AssertIsLabeledStatement();
            if (!gotoStmt.WasCompilerGenerated && !targetStmt.WasCompilerGenerated && !RegionContains(gotoStmt.Syntax.Span) && RegionContains(targetStmt.Syntax.Span))
            {
                pending.State = ResetState(pending.State);
            }

            base.NoteBranch(pending, gotoStmt, targetStmt);
        }

        public override BoundNode VisitRangeVariable(BoundRangeVariable node)
        {
            if (IsInside && !RegionContains(node.RangeVariableSymbol.GetFirstLocation().SourceSpan))
            {
                _dataFlowsIn.Add(node.RangeVariableSymbol);
            }

            return null;
        }

        protected override void ReportUnassigned(Symbol symbol, SyntaxNode node, int slot, bool skipIfUseBeforeDeclaration)
        {
            // TODO: how to handle fields of structs?
            if (RegionContains(node.Span))
            {
                // if the field access is reported as unassigned it should mean the original local
                // or parameter flows in, so we should get the symbol associated with the expression
                _dataFlowsIn.Add(symbol.Kind == SymbolKind.Field ? GetNonMemberSymbol(slot) : symbol);
            }

            base.ReportUnassigned(symbol, node, slot, skipIfUseBeforeDeclaration);
        }

        protected override void ReportUnassignedOutParameter(
            ParameterSymbol parameter,
            SyntaxNode node,
            Location location)
        {
            if (node != null && node is ReturnStatementSyntax && RegionContains(node.Span))
            {
                _dataFlowsIn.Add(parameter);
            }

            base.ReportUnassignedOutParameter(parameter, node, location);
        }
    }
}
