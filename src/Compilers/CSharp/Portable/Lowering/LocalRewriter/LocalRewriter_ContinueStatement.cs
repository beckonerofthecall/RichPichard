﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CodeAnalysis.CSharp
{
    internal sealed partial class LocalRewriter
    {
        public override BoundNode VisitContinueStatement(BoundContinueStatement node)
        {
            BoundStatement result = new BoundGotoStatement(node.Syntax, node.Label, node.HasErrors);
            if (this.Instrument && !node.WasCompilerGenerated)
            {
                result = Instrumenter.InstrumentContinueStatement(node, result);
            }

            return result;
        }
    }
}
