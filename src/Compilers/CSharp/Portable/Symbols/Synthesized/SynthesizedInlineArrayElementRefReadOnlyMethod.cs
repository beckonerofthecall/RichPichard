﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols
{
    internal sealed class SynthesizedInlineArrayElementRefReadOnlyMethod : SynthesizedGlobalMethodSymbol
    {
        internal SynthesizedInlineArrayElementRefReadOnlyMethod(SynthesizedPrivateImplementationDetailsType privateImplType, string synthesizedMethodName, NamedTypeSymbol intType)
            : base(privateImplType, synthesizedMethodName)
        {
            this.SetTypeParameters(ImmutableArray.Create<TypeParameterSymbol>(new SynthesizedSimpleMethodTypeParameterSymbol(this, 0, "TBuffer"), new SynthesizedSimpleMethodTypeParameterSymbol(this, 1, "TElement")));
            this.SetReturnType(TypeParameters[1]);
            this.SetParameters(ImmutableArray.Create<ParameterSymbol>(SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(TypeParameters[0]), 0, RefKind.In, "buffer"),
                                                                      SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(intType), 1, RefKind.None, "index")));
        }

        public override RefKind RefKind => RefKind.RefReadOnly;

        internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
        {
            SyntheticBoundNodeFactory f = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
            f.CurrentFunction = this;

            try
            {
                // return ref Unsafe.Add<TElement>(ref Unsafe.As<TBuffer, TElement>(ref Unsafe.AsRef<TBuffer>(in buffer)), index)

                var body = f.Return(f.Call(null,
                                           f.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_Unsafe__Add_T).Construct(TypeParameters[1]),
                                           f.Call(null,
                                                  f.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_Unsafe__As_T).Construct(ImmutableArray<TypeSymbol>.CastUp(TypeParameters)),
                                                  f.Call(null,
                                                         f.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_Unsafe__AsRef_T).Construct(TypeParameters[0]),
                                                         f.Parameter(Parameters[0]))),
                                           f.Parameter(Parameters[1])));

                // NOTE: we created this block in its most-lowered form, so analysis is unnecessary
                f.CloseMethod(body);
            }
            catch (SyntheticBoundNodeFactory.MissingPredefinedMember ex)
            {
                diagnostics.Add(ex.Diagnostic);
                f.CloseMethod(f.ThrowNull());
            }
        }
    }
}
