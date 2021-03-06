﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using StarkPlatform.CodeAnalysis.Host;

namespace StarkPlatform.CodeAnalysis.Diagnostics
{
    internal interface IDiagnosticPropertiesService : ILanguageService
    {
        ImmutableDictionary<string, string> GetAdditionalProperties(Diagnostic diagnostic);
    }
}
