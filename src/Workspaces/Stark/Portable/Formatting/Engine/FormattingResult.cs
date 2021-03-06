﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using StarkPlatform.CodeAnalysis.Formatting;
using StarkPlatform.CodeAnalysis.Shared.Collections;
using StarkPlatform.CodeAnalysis.Text;

namespace StarkPlatform.CodeAnalysis.Stark.Formatting
{
    /// <summary>
    /// this holds onto changes made by formatting engine.
    /// </summary>
    internal class FormattingResult : AbstractFormattingResult
    {
        internal FormattingResult(TreeData treeInfo, TokenStream tokenStream, TextSpan spanToFormat) :
            base(treeInfo, tokenStream, spanToFormat)
        {
        }

        protected override SyntaxNode Rewriter(Dictionary<ValueTuple<SyntaxToken, SyntaxToken>, TriviaData> changeMap, CancellationToken cancellationToken)
        {
            var rewriter = new TriviaRewriter(this.TreeInfo.Root, SimpleIntervalTree.Create(TextSpanIntervalIntrospector.Instance, this.FormattedSpan), changeMap, cancellationToken);
            return rewriter.Transform();
        }
    }
}
