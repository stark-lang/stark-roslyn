﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace StarkPlatform.CodeAnalysis.Editor.Commands
{
    /// <summary>
    /// Arguments for the copy to interactive command.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class CopyToInteractiveCommandArgs : CommandArgs
    {
        public CopyToInteractiveCommandArgs(ITextView textView, ITextBuffer subjectBuffer)
            : base(textView, subjectBuffer)
        {
        }
    }
}
