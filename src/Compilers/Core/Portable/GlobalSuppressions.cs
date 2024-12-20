﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Microsoft.CodeAnalysis.Compilation.CreateTupleTypeSymbol(System.Collections.Immutable.ImmutableArray{Microsoft.CodeAnalysis.ITypeSymbol},System.Collections.Immutable.ImmutableArray{System.String})~Microsoft.CodeAnalysis.INamedTypeSymbol")]
[assembly: SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Microsoft.CodeAnalysis.Compilation.CreateTupleTypeSymbol(Microsoft.CodeAnalysis.INamedTypeSymbol,System.Collections.Immutable.ImmutableArray{System.String})~Microsoft.CodeAnalysis.INamedTypeSymbol")]

// These arise from modifications to existing API. The analysis seems too strict as the first
// required parameter type is different for all overloads and so there is no ambiguity. 
[assembly: SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Microsoft.CodeAnalysis.Text.SourceText.From(System.IO.Stream,System.Text.Encoding,Microsoft.CodeAnalysis.Text.SourceHashAlgorithm,System.Boolean,System.Boolean)~Microsoft.CodeAnalysis.Text.SourceText")]
[assembly: SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Microsoft.CodeAnalysis.Text.SourceText.From(System.Byte[],System.Int32,System.Text.Encoding,Microsoft.CodeAnalysis.Text.SourceHashAlgorithm,System.Boolean,System.Boolean)~Microsoft.CodeAnalysis.Text.SourceText")]
[assembly: SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Microsoft.CodeAnalysis.Text.SourceText.From(System.IO.TextReader,System.Int32,System.Text.Encoding,Microsoft.CodeAnalysis.Text.SourceHashAlgorithm)~Microsoft.CodeAnalysis.Text.SourceText")]
[assembly: SuppressMessage("ApiDesign", "RS0027:Public API with optional parameter(s) should have the most parameters amongst its public overloads.", Justification = "<Pending>", Scope = "member", Target = "~M:Microsoft.CodeAnalysis.Text.SourceText.From(System.String,System.Text.Encoding,Microsoft.CodeAnalysis.Text.SourceHashAlgorithm)~Microsoft.CodeAnalysis.Text.SourceText")]
[assembly: SuppressMessage("Documentation", "CA1200:Avoid using cref tags with a prefix", Justification = "Linking docs to member not in this layer", Scope = "member", Target = "~P:Microsoft.CodeAnalysis.Operations.IConversionOperation.Conversion")]
[assembly: SuppressMessage("ApiDesign", "RS0016:Add public types and members to the declared API", Justification = "<Pending>", Scope = "type", Target = "~T:Microsoft.CodeAnalysis.SpecialType")]
