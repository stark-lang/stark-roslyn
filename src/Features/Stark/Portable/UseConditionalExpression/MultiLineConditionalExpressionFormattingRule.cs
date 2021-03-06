﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using StarkPlatform.CodeAnalysis.Stark.Syntax;
using StarkPlatform.CodeAnalysis.Formatting.Rules;
using StarkPlatform.CodeAnalysis.Options;
using static StarkPlatform.CodeAnalysis.UseConditionalExpression.UseConditionalExpressionHelpers;

namespace StarkPlatform.CodeAnalysis.Stark.UseConditionalExpression
{
    /// <summary>
    /// Special formatting rule that will convert a conditional expression into the following
    /// form if it has the <see cref="SpecializedFormattingAnnotation"/> on it:
    /// 
    /// <code>
    ///     var v = expr
    ///         ? whenTrue
    ///         : whenFalse
    /// </code>
    /// 
    /// i.e. both branches will be on a newline, indented once from the parent indentation.
    /// </summary>
    internal class MultiLineConditionalExpressionFormattingRule : AbstractFormattingRule
    {
        public static readonly IFormattingRule Instance = new MultiLineConditionalExpressionFormattingRule();

        private MultiLineConditionalExpressionFormattingRule()
        {
        }

        private bool IsQuestionOrColonOfNewConditional(SyntaxToken token)
        {
            if (token.Kind() == SyntaxKind.QuestionToken ||
                token.Kind() == SyntaxKind.ColonToken)
            {
                return token.Parent.HasAnnotation(SpecializedFormattingAnnotation);
            }

            return false;
        }

        public override AdjustNewLinesOperation GetAdjustNewLinesOperation(
            SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustNewLinesOperation> nextOperation)
        {
            if (IsQuestionOrColonOfNewConditional(currentToken))
            {
                // We want to force the ? and : to each be put onto the following line.
                return FormattingOperations.CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.ForceLines);
            }

            return nextOperation.Invoke();
        }

        public override void AddIndentBlockOperations(
            List<IndentBlockOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<IndentBlockOperation> nextOperation)
        {
            if (node.HasAnnotation(SpecializedFormattingAnnotation) &&
                node is IfExpressionSyntax conditional)
            {
                var statement = conditional.FirstAncestorOrSelf<StatementSyntax>();
                if (statement != null)
                {
                    var baseToken = statement.GetFirstToken();

                    // we want to indent the ? and : in one level from the containing statement.
                    list.Add(FormattingOperations.CreateRelativeIndentBlockOperation(
                        baseToken, conditional.ThenKeyword, conditional.WhenTrue.GetLastToken(),
                        indentationDelta: 1, IndentBlockOption.RelativeToFirstTokenOnBaseTokenLine));
                    list.Add(FormattingOperations.CreateRelativeIndentBlockOperation(
                        baseToken, conditional.ElseKeyword, conditional.WhenFalse.GetLastToken(),
                        indentationDelta: 1, IndentBlockOption.RelativeToFirstTokenOnBaseTokenLine));
                    return;
                }
            }

            nextOperation.Invoke(list);
        }
    }
}
