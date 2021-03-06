﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using StarkPlatform.CodeAnalysis.Stark.Extensions.ContextQuery;

namespace StarkPlatform.CodeAnalysis.Stark.Completion.KeywordRecommenders
{
    internal class DefaultKeywordRecommender : AbstractSyntacticSingleKeywordRecommender
    {
        public DefaultKeywordRecommender()
            : base(SyntaxKind.DefaultKeyword, isValidInPreprocessorContext: true)
        {
        }

        protected override bool IsValidContext(int position, CSharpSyntaxContext context, CancellationToken cancellationToken)
        {
            return
                IsValidPreProcessorContext(context) ||
                context.IsStatementContext ||
                context.IsGlobalStatementContext ||
                context.IsAnyExpressionContext ||
                context.TargetToken.IsSwitchLabelContext();
        }

        private static bool IsValidPreProcessorContext(CSharpSyntaxContext context)
        {
            // cases:
            //   #line |
            //   #line d|
            //   # line |
            //   # line d|

            var previousToken1 = context.TargetToken;
            var previousToken2 = previousToken1.GetPreviousToken(includeSkipped: true);

            return
                previousToken1.Kind() == SyntaxKind.LineKeyword &&
                previousToken2.Kind() == SyntaxKind.HashToken;
        }
    }
}
