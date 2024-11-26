// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.CodeAnalysis.BuildTasks
{
    /// <summary>
    /// Logs a localizable message for implicitly skipping analyzers for implicitly triggered builds.
    /// </summary>
    public sealed class ShowMessageForImplicitlySkipAnalyzers : Task
    {
        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, ErrorString.ImplicitlySkipAnalyzersMessage);
            return true;
        }
    }
}
