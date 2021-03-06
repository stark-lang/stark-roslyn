﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using StarkPlatform.CodeAnalysis;
using StarkPlatform.CodeAnalysis.Stark.Formatting;
using StarkPlatform.CodeAnalysis.Options;
using StarkPlatform.VisualStudio.LanguageServices.Implementation.Options;

namespace StarkPlatform.VisualStudio.LanguageServices.CSharp.Options.Formatting
{
    /// <summary>
    /// Interaction logic for FormattingWrappingOptionPage.xaml
    /// </summary>
    internal class WrappingViewModel : AbstractOptionPreviewViewModel
    {
        private const string s_blockPreview = @"
class C
{
//[
    public int Goo { get; set; }
//]    
}";

        private const string s_declarationPreview = @"
class C{
    void goo()
    {
//[
        int i = 0; string name = ""John"";
//]
    }
}";

        public WrappingViewModel(OptionSet options, IServiceProvider serviceProvider) : base(options, serviceProvider, LanguageNames.Stark)
        {
            Items.Add(new CheckBoxOptionViewModel(CSharpFormattingOptions.WrappingPreserveSingleLine, CSharpVSResources.Leave_block_on_single_line, s_blockPreview, this, options));
            Items.Add(new CheckBoxOptionViewModel(CSharpFormattingOptions.WrappingKeepStatementsOnSingleLine, CSharpVSResources.Leave_statements_and_member_declarations_on_the_same_line, s_declarationPreview, this, options));
        }
    }
}
