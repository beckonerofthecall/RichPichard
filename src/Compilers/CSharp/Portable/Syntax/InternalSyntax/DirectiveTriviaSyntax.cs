﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax
{
    internal partial class DirectiveTriviaSyntax
    {
        internal override DirectiveStack ApplyDirectives(DirectiveStack stack)
        {
            return stack.Add(new Directive(this));
        }

        public sealed override bool IsDirective => true;
    }
}
