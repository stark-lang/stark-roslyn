﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.CodeAnalysis.Host;

namespace StarkPlatform.CodeAnalysis.SemanticModelWorkspaceService
{
    /// <summary>
    /// a service that provides a semantic model that will re-use last known compilation if
    /// semantic version hasn't changed.
    /// </summary>
    internal interface ISemanticModelService : IWorkspaceService
    {
        /// <summary>
        /// Don't call this directly. use Document extension method GetSemanticModelForNodeAsync or GetSemanticModelForSpanAsync instead.
        /// 
        /// see the descriptions on the extension methods
        /// </summary>
        Task<SemanticModel> GetSemanticModelForNodeAsync(Document document, SyntaxNode node, CancellationToken cancellationToken);
    }
}
