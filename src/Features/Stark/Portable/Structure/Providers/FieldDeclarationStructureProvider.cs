﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using StarkPlatform.CodeAnalysis.Stark.Syntax;
using StarkPlatform.CodeAnalysis.Options;
using StarkPlatform.CodeAnalysis.PooledObjects;
using StarkPlatform.CodeAnalysis.Structure;

namespace StarkPlatform.CodeAnalysis.Stark.Structure
{
    internal class FieldDeclarationStructureProvider : AbstractSyntaxNodeStructureProvider<FieldDeclarationSyntax>
    {
        protected override void CollectBlockSpans(
            FieldDeclarationSyntax fieldDeclaration,
            ArrayBuilder<BlockSpan> spans,
            OptionSet options,
            CancellationToken cancellationToken)
        {
            CSharpStructureHelpers.CollectCommentBlockSpans(fieldDeclaration, spans);
        }
    }
}
