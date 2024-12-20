﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.FlowAnalysis
{
    internal sealed partial class ControlFlowGraphBuilder
    {
        /// <summary>
        /// This structure is meant to capture a snapshot of the <see cref="ControlFlowGraphBuilder"/> state
        /// that is needed to build graphs for lambdas and local functions.
        /// </summary>
        internal readonly struct Context
        {
            public readonly IOperation? ImplicitInstance;
            public readonly INamedTypeSymbol? AnonymousType;
            public readonly ImmutableArray<KeyValuePair<IPropertySymbol, IOperation>> AnonymousTypePropertyValues;

            internal Context(IOperation? implicitInstance, INamedTypeSymbol? anonymousType, ImmutableArray<KeyValuePair<IPropertySymbol, IOperation>> anonymousTypePropertyValues)
            {
                Debug.Assert(!anonymousTypePropertyValues.IsDefault);
                Debug.Assert(implicitInstance == null || anonymousType == null);
                ImplicitInstance = implicitInstance;
                AnonymousType = anonymousType;
                AnonymousTypePropertyValues = anonymousTypePropertyValues;
            }
        }

        private Context GetCurrentContext()
        {
            return new Context(_currentImplicitInstance.ImplicitInstance, _currentImplicitInstance.AnonymousType,
                               _currentImplicitInstance.AnonymousTypePropertyValues?.ToImmutableArray() ??
                                   ImmutableArray<KeyValuePair<IPropertySymbol, IOperation>>.Empty);
        }

        private void SetCurrentContext(in Context context)
        {
            _currentImplicitInstance = new ImplicitInstanceInfo(in context);
        }
    }
}
