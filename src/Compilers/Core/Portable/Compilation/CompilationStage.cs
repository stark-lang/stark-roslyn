﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using StarkPlatform.CodeAnalysis.Text;

namespace StarkPlatform.CodeAnalysis
{
    /// <summary>
    /// Represents the possible compilation stages for which it is possible to get diagnostics
    /// (errors).
    /// </summary>
    internal enum CompilationStage
    {
        Parse,
        Declare,
        Compile,
        Emit
    }
}
