﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.CodeAnalysis;
using StarkPlatform.CodeAnalysis.Stark.CodeStyle;
using StarkPlatform.CodeAnalysis.Stark.Symbols;
using StarkPlatform.CodeAnalysis.Stark.Syntax;
using StarkPlatform.CodeAnalysis.ErrorReporting;
using StarkPlatform.CodeAnalysis.Options;
using StarkPlatform.CodeAnalysis.Shared.Extensions;
using StarkPlatform.CodeAnalysis.Simplification;
using Roslyn.Utilities;

namespace StarkPlatform.CodeAnalysis.Stark.Extensions
{
    internal static partial class ITypeSymbolExtensions
    {
        public static ExpressionSyntax GenerateExpressionSyntax(
            this ITypeSymbol typeSymbol)
        {
            return typeSymbol.Accept(ExpressionSyntaxGeneratorVisitor.Instance).WithAdditionalAnnotations(Simplifier.Annotation);
        }

        public static NameSyntax GenerateNameSyntax(
            this INamespaceOrTypeSymbol symbol, bool allowVar = true)
        {
            return (NameSyntax)GenerateTypeSyntax(symbol, nameSyntax: true, allowVar: allowVar);
        }

        public static TypeSyntax GenerateTypeSyntax(
            this INamespaceOrTypeSymbol symbol, bool allowVar = true)
        {
            return GenerateTypeSyntax(symbol, nameSyntax: false, allowVar: allowVar);
        }

        private static TypeSyntax GenerateTypeSyntax(
            INamespaceOrTypeSymbol symbol, bool nameSyntax, bool allowVar = true)
        {
            if (symbol is ITypeSymbol type && type.ContainsAnonymousType())
            {
                // something with an anonymous type can only be represented with 'var', regardless
                // of what the user's preferences might be.
                return SyntaxFactory.IdentifierName("var");
            }

            var syntax = symbol.Accept(TypeSyntaxGeneratorVisitor.Create(nameSyntax))
                               .WithAdditionalAnnotations(Simplifier.Annotation);

            if (!allowVar)
            {
                syntax = syntax.WithAdditionalAnnotations(DoNotAllowVarAnnotation.Annotation);
            }

            return syntax;
        }

        public static TypeSyntax GenerateRefTypeSyntax(
            this INamespaceOrTypeSymbol symbol)
        {
            var underlyingType = GenerateTypeSyntax(symbol)
                .WithPrependedLeadingTrivia(SyntaxFactory.ElasticMarker)
                .WithAdditionalAnnotations(Simplifier.Annotation);
            var refKeyword = SyntaxFactory.Token(SyntaxKind.RefKeyword);
            return SyntaxFactory.RefType(refKeyword, underlyingType);
        }

        public static TypeSyntax GenerateRefReadOnlyTypeSyntax(
            this INamespaceOrTypeSymbol symbol)
        {
            var underlyingType = GenerateTypeSyntax(symbol)
                .WithPrependedLeadingTrivia(SyntaxFactory.ElasticMarker)
                .WithAdditionalAnnotations(Simplifier.Annotation);
            var refKeyword = SyntaxFactory.Token(SyntaxKind.RefKeyword);
            var readOnlyKeyword = SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword);
            return SyntaxFactory.RefType(refKeyword, SyntaxFactory.SimpleExtendedType(SyntaxTokenList.Create(readOnlyKeyword), underlyingType));
        }

        public static bool ContainingTypesOrSelfHasUnsafeKeyword(this ITypeSymbol containingType)
        {
            do
            {
                foreach (var reference in containingType.DeclaringSyntaxReferences)
                {
                    if (reference.GetSyntax().ChildTokens().Any(t => t.IsKind(SyntaxKind.UnsafeKeyword)))
                    {
                        return true;
                    }
                }

                containingType = containingType.ContainingType;
            }
            while (containingType != null);
            return false;
        }

        public static async Task<ISymbol> FindApplicableAlias(this ITypeSymbol type, int position, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            try
            {
                if (semanticModel.IsSpeculativeSemanticModel)
                {
                    position = semanticModel.OriginalPositionForSpeculation;
                    semanticModel = semanticModel.ParentModel;
                }

                var root = await semanticModel.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

                IEnumerable<ImportDirectiveSyntax> applicableUsings = GetApplicableUsings(position, root as CompilationUnitSyntax);
                foreach (var applicableUsing in applicableUsings)
                {
                    var alias = semanticModel.GetOriginalSemanticModel().GetDeclaredSymbol(applicableUsing, cancellationToken);
                    if (alias != null && alias.Target == type)
                    {
                        return alias;
                    }
                }

                return null;
            }
            catch (Exception e) when (FatalError.ReportUnlessCanceled(e))
            {
                throw ExceptionUtilities.Unreachable;
            }
        }

        private static IEnumerable<ImportDirectiveSyntax> GetApplicableUsings(int position, SyntaxNode root)
        {
            var namespaceUsings = root.FindToken(position).Parent.GetAncestors<NamespaceDeclarationSyntax>().SelectMany(n => n.Usings);
            var allUsings = root is CompilationUnitSyntax
                ? ((CompilationUnitSyntax)root).Usings.Concat(namespaceUsings)
                : namespaceUsings;
            return allUsings.Where(u => u.Alias != null);
        }

        public static bool IsIntrinsicType(this ITypeSymbol typeSymbol)
        {
            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Char:
                case SpecialType.System_Int8:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt8:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_Float32:
                case SpecialType.System_Float64:
                // NOTE: VB treats System.DateTime as an intrinsic, while C# does not, see "predeftype.h"
                //case SpecialType.System_DateTime:
                case SpecialType.System_Decimal:
                    return true;
                default:
                    return false;
            }
        }
    }
}
