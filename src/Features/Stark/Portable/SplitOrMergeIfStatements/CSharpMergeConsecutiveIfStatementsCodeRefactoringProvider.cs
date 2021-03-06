﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using StarkPlatform.CodeAnalysis.CodeRefactorings;
using StarkPlatform.CodeAnalysis.Stark.Syntax;
using StarkPlatform.CodeAnalysis.Shared.Extensions;
using StarkPlatform.CodeAnalysis.SplitOrMergeIfStatements;
using StarkPlatform.CodeAnalysis.Text;

namespace StarkPlatform.CodeAnalysis.Stark.SplitOrMergeIfStatements
{
    [ExportCodeRefactoringProvider(LanguageNames.Stark, Name = PredefinedCodeRefactoringProviderNames.MergeConsecutiveIfStatements), Shared]
    internal sealed class CSharpMergeConsecutiveIfStatementsCodeRefactoringProvider
        : AbstractMergeConsecutiveIfStatementsCodeRefactoringProvider
    {
        protected override bool IsApplicableSpan(SyntaxNode node, TextSpan span, out SyntaxNode ifOrElseIf)
        {
            if (node is IfStatementSyntax ifStatement)
            {
                // Cases:
                // 1. Position is at a child token of an if statement with no selection (e.g. 'if' keyword, a parenthesis)
                // 2. Selection around the 'if' keyword
                // 3. Selection around the header - from 'if' keyword to the end of the condition
                // 4. Selection around the whole if statement *excluding* its else clause - from 'if' keyword to the end of its statement
                if (span.Length == 0 ||
                    span.IsAround(ifStatement.IfKeyword) ||
                    span.IsAround(ifStatement.IfKeyword, ifStatement.Condition.GetLastToken()) ||
                    span.IsAround(ifStatement.IfKeyword, ifStatement.Statement))
                {
                    ifOrElseIf = ifStatement;
                    return true;
                }
            }

            if (node is ElseClauseSyntax elseClause && elseClause.Statement is IfStatementSyntax elseIfStatement)
            {
                // 5. Position is at a child token of an else clause with no selection ('else' keyword)
                // 6. Selection around the header including the 'else' keyword - from 'else' keyword to the end of the condition
                // 7. Selection from the 'else' keyword to the end of the if statement's statement
                if (span.Length == 0 ||
                    span.IsAround(elseClause.ElseKeyword, elseIfStatement.GetLastToken()) ||
                    span.IsAround(elseClause.ElseKeyword, elseIfStatement.Statement))
                {
                    ifOrElseIf = elseIfStatement;
                    return true;
                }
            }

            ifOrElseIf = null;
            return false;
        }
    }
}
