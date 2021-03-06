﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using StarkPlatform.CodeAnalysis.Stark.Extensions;
using StarkPlatform.CodeAnalysis.Stark.Syntax;
using StarkPlatform.CodeAnalysis.LanguageServices;
using StarkPlatform.CodeAnalysis.Options;
using StarkPlatform.CodeAnalysis.PooledObjects;
using StarkPlatform.CodeAnalysis.Shared.Extensions;
using StarkPlatform.CodeAnalysis.Simplification;
using Roslyn.Utilities;

namespace StarkPlatform.CodeAnalysis.Stark.Simplification
{
    internal partial class CSharpParenthesesReducer : AbstractCSharpReducer
    {
        private static readonly ObjectPool<IReductionRewriter> s_pool = new ObjectPool<IReductionRewriter>(
            () => new Rewriter(s_pool));

        public CSharpParenthesesReducer() : base(s_pool)
        {
        }

        private static readonly Func<ParenthesizedExpressionSyntax, SemanticModel, OptionSet, CancellationToken, SyntaxNode> s_simplifyParentheses = SimplifyParentheses;

        private static SyntaxNode SimplifyParentheses(
            ParenthesizedExpressionSyntax node,
            SemanticModel semanticModel,
            OptionSet optionSet,
            CancellationToken cancellationToken)
        {
            if (node.CanRemoveParentheses(semanticModel))
            {
                // TODO(DustinCa): We should not be skipping elastic trivia below.
                // However, the formatter seems to mess up trailing trivia in some
                // cases if elastic trivia is there -- and it's not clear why.
                // Specifically remove the elastic trivia formatting rule doesn't
                // have any effect.
                var resultNode = CSharpSyntaxFactsService.Instance.Unparenthesize(node);
                return SimplificationHelpers.CopyAnnotations(from: node, to: resultNode);
            }

            // We don't know how to simplify this.
            return node;
        }
    }
}
