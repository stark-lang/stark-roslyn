﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Threading;
using StarkPlatform.CodeAnalysis.Stark.Syntax;
using StarkPlatform.CodeAnalysis.Organizing.Organizers;

namespace StarkPlatform.CodeAnalysis.Stark.Organizing.Organizers
{
    [ExportSyntaxNodeOrganizer(LanguageNames.Stark), Shared]
    internal class OperatorDeclarationOrganizer : AbstractSyntaxNodeOrganizer<OperatorDeclarationSyntax>
    {
        protected override OperatorDeclarationSyntax Organize(
            OperatorDeclarationSyntax syntax,
            CancellationToken cancellationToken)
        {
            return syntax.Update(syntax.AttributeLists,
                ModifiersOrganizer.Organize(syntax.Modifiers),
                syntax.ReturnType,
                syntax.OperatorKeyword,
                syntax.OperatorToken,
                syntax.ParameterList,
                syntax.Body,
                syntax.EosToken);
        }
    }
}
