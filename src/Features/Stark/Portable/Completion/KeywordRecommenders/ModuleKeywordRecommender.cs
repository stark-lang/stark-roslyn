﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using StarkPlatform.CodeAnalysis.Stark.Extensions;
using StarkPlatform.CodeAnalysis.Stark.Extensions.ContextQuery;
using StarkPlatform.CodeAnalysis.Stark.Syntax;
using StarkPlatform.CodeAnalysis.Shared.Extensions;

namespace StarkPlatform.CodeAnalysis.Stark.Completion.KeywordRecommenders
{
    internal class ModuleKeywordRecommender : AbstractSyntacticSingleKeywordRecommender
    {
        public ModuleKeywordRecommender()
            : base(SyntaxKind.ModuleKeyword)
        {
        }

        protected override bool IsValidContext(int position, CSharpSyntaxContext context, CancellationToken cancellationToken)
        {
            if (context.IsTypeAttributeContext(cancellationToken))
            {
                var token = context.LeftToken;
                var type = token.GetAncestor<MemberDeclarationSyntax>();

                return type == null || type.IsParentKind(SyntaxKind.CompilationUnit);
            }

            return false;
        }
    }
}
