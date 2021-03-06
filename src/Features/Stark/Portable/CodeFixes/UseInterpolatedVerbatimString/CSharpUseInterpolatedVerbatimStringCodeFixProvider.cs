﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.CodeAnalysis.CodeActions;
using StarkPlatform.CodeAnalysis.CodeFixes;
using StarkPlatform.CodeAnalysis.Stark.Syntax;
using StarkPlatform.CodeAnalysis.Editing;
using StarkPlatform.CodeAnalysis.Shared.Extensions;
using Roslyn.Utilities;

namespace StarkPlatform.CodeAnalysis.Stark.UseInterpolatedVerbatimString
{
    /// <summary>
    /// Converts a verbatim interpolated string @$"" to an interpolated verbatim string $@""
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.Stark), Shared]
    internal partial class CSharpUseInterpolatedVerbatimStringCodeFixProvider : SyntaxEditorBasedCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create("CS8401");

        private const string InterpolatedVerbatimText = "$@\"";

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(new MyCodeAction(
                c => FixAsync(context.Document, context.Diagnostics.First(), c)),
                context.Diagnostics);
            return Task.CompletedTask;
        }

        protected override Task FixAllAsync(
            Document document, ImmutableArray<Diagnostic> diagnostics,
            SyntaxEditor editor, CancellationToken cancellationToken)
        {
            foreach (var diagnostic in diagnostics)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AddEdits(editor, diagnostic, cancellationToken);
            }

            return Task.CompletedTask;
        }

        private void AddEdits(
            SyntaxEditor editor,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var verbatimInterpolatedLocation = diagnostic.Location;
            var verbatimInterpolated = (InterpolatedStringExpressionSyntax)verbatimInterpolatedLocation.FindNode(getInnermostNodeForTie: true, cancellationToken);

            var oldStartToken = verbatimInterpolated.StringStartToken;
            var newStartToken = SyntaxFactory.Token(oldStartToken.LeadingTrivia, SyntaxKind.InterpolatedVerbatimStringStartToken,
                InterpolatedVerbatimText, InterpolatedVerbatimText, oldStartToken.TrailingTrivia);

            var interpolatedVerbatim = verbatimInterpolated.WithStringStartToken(newStartToken);

            editor.ReplaceNode(verbatimInterpolated, interpolatedVerbatim);
        }

        private class MyCodeAction : CodeAction.DocumentChangeAction
        {
            public MyCodeAction(Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(FeaturesResources.Use_interpolated_verbatim_string, createChangedDocument)
            {
            }
        }
    }
}
