﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.CodeAnalysis.FindSymbols;
using StarkPlatform.CodeAnalysis.LanguageServices;
using StarkPlatform.CodeAnalysis.Shared.Extensions;
using StarkPlatform.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace StarkPlatform.CodeAnalysis.MoveDeclarationNearReference
{
    internal partial class AbstractMoveDeclarationNearReferenceService<
        TService,
        TStatementSyntax,
        TLocalDeclarationStatementSyntax,
        TVariableDeclarationSyntax>
    {
        private class State
        {
            public TLocalDeclarationStatementSyntax DeclarationStatement { get; private set; }
            public TVariableDeclarationSyntax VariableDeclaration { get; private set; }
            public ILocalSymbol LocalSymbol { get; private set; }
            public SyntaxNode OutermostBlock { get; private set; }
            public SyntaxNode InnermostBlock { get; private set; }
            public SyntaxList<TStatementSyntax> OutermostBlockStatements { get; private set; }
            public SyntaxList<TStatementSyntax> InnermostBlockStatements { get; private set; }
            public TStatementSyntax FirstStatementAffectedInInnermostBlock { get; private set; }
            public int IndexOfDeclarationStatementInInnermostBlock { get; private set; }
            public int IndexOfFirstStatementAffectedInInnermostBlock { get; private set; }

            internal static async Task<State> GenerateAsync(
                TService service,
                Document document,
                TLocalDeclarationStatementSyntax statement,
                CancellationToken cancellationToken)
            {
                var state = new State();
                if (!await state.TryInitializeAsync(service, document, statement, cancellationToken).ConfigureAwait(false))
                {
                    return null;
                }

                return state;
            }

            private async Task<bool> TryInitializeAsync(
                TService service,
                Document document,
                TLocalDeclarationStatementSyntax node,
                CancellationToken cancellationToken)
            {
                var syntaxFacts = document.GetLanguageService<ISyntaxFactsService>();

                this.DeclarationStatement = node;

                var variables = syntaxFacts.GetVariablesOfLocalDeclarationStatement(this.DeclarationStatement);
                this.VariableDeclaration = (TVariableDeclarationSyntax)variables;
                if (!service.IsValidVariableDeclaration(this.VariableDeclaration))
                {
                    return false;
                }

                this.OutermostBlock = this.DeclarationStatement.Parent;
                if (!syntaxFacts.IsExecutableBlock(this.OutermostBlock))
                {
                    return false;
                }

                var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
                this.LocalSymbol = (ILocalSymbol)semanticModel.GetDeclaredSymbol(
                    service.GetVariableDeclarationSymbolNode(this.VariableDeclaration), cancellationToken);
                if (this.LocalSymbol == null)
                {
                    // This can happen in broken code, for example: "{ object x; object }"
                    return false;
                }

                var findReferencesResult = await SymbolFinder.FindReferencesAsync(this.LocalSymbol, document.Project.Solution, cancellationToken).ConfigureAwait(false);
                var findReferencesList = findReferencesResult.ToList();
                if (findReferencesList.Count != 1)
                {
                    return false;
                }

                var references = findReferencesList[0].Locations.ToList();
                if (references.Count == 0)
                {
                    return false;
                }

                var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                var referencingStatements =
                    (from r in references
                     let token = syntaxRoot.FindToken(r.Location.SourceSpan.Start)
                     let statement = token.GetAncestor<TStatementSyntax>()
                     where statement != null
                     select statement).ToSet();

                if (referencingStatements.Count == 0)
                {
                    return false;
                }

                this.InnermostBlock = syntaxFacts.FindInnermostCommonExecutableBlock(referencingStatements);
                if (this.InnermostBlock == null)
                {
                    return false;
                }

                this.InnermostBlockStatements = syntaxFacts.GetExecutableBlockStatements(this.InnermostBlock);
                this.OutermostBlockStatements = syntaxFacts.GetExecutableBlockStatements(this.OutermostBlock);

                var allAffectedStatements = new HashSet<TStatementSyntax>(referencingStatements.SelectMany(
                    expr => expr.GetAncestorsOrThis<TStatementSyntax>()));
                this.FirstStatementAffectedInInnermostBlock = this.InnermostBlockStatements.FirstOrDefault(allAffectedStatements.Contains);

                if (this.FirstStatementAffectedInInnermostBlock == null)
                {
                    return false;
                }

                if (this.FirstStatementAffectedInInnermostBlock == this.DeclarationStatement)
                {
                    return false;
                }

                this.IndexOfDeclarationStatementInInnermostBlock = this.InnermostBlockStatements.IndexOf(this.DeclarationStatement);
                this.IndexOfFirstStatementAffectedInInnermostBlock = this.InnermostBlockStatements.IndexOf(this.FirstStatementAffectedInInnermostBlock);
                if (this.IndexOfDeclarationStatementInInnermostBlock >= 0 &&
                    this.IndexOfDeclarationStatementInInnermostBlock < this.IndexOfFirstStatementAffectedInInnermostBlock)
                {
                    // Don't want to move a decl with initializer past other decls in order to move it to the first
                    // affected statement.  If we do we can end up in the following situation: 
#if false
                    int x = 0;
                    int y = 0;
                    Console.WriteLine(x + y);
#endif
                    // Each of these declarations will want to 'move' down to the WriteLine
                    // statement and we don't want to keep offering the refactoring.  Note: this
                    // solution is overly aggressive.  Technically if 'y' weren't referenced in
                    // Console.Writeline, then it might be a good idea to move the 'x'.  But this
                    // gives good enough behavior most of the time.

                    // Note that if the variable declaration has no initializer, then we still want to offer
                    // the move as the closest reference will be an assignment to the variable
                    // and we should be able to merge the declaration and assignment into a single
                    // statement.
                    // So, we also check if the variable declaration has an initializer below.

                    if (syntaxFacts.GetInitializerOfVariableDeclaration(this.VariableDeclaration) != null &&
                        InDeclarationStatementGroup(this.IndexOfDeclarationStatementInInnermostBlock, this.IndexOfFirstStatementAffectedInInnermostBlock))
                    {
                        return false;
                    }
                }

                var previousToken = FirstStatementAffectedInInnermostBlock.GetFirstToken().GetPreviousToken();
                var affectedSpan = TextSpan.FromBounds(previousToken.SpanStart, FirstStatementAffectedInInnermostBlock.Span.End);
                if (semanticModel.SyntaxTree.OverlapsHiddenPosition(affectedSpan, cancellationToken))
                {
                    return false;
                }

                return true;
            }

            private bool InDeclarationStatementGroup(
                int originalIndexInBlock, int firstStatementIndexAffectedInBlock)
            {
                for (var i = originalIndexInBlock; i < firstStatementIndexAffectedInBlock; i++)
                {
                    if (!(this.InnermostBlockStatements[i] is TLocalDeclarationStatementSyntax))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
