﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using StarkPlatform.CodeAnalysis.Navigation;
using StarkPlatform.CodeAnalysis.Text;

namespace StarkPlatform.CodeAnalysis.NavigateTo
{
    internal interface INavigateToSearchResult
    {
        string AdditionalInformation { get; }
        string Kind { get; }
        NavigateToMatchKind MatchKind { get; }
        bool IsCaseSensitive { get; }
        string Name { get; }
        ImmutableArray<TextSpan> NameMatchSpans { get; }
        string SecondarySort { get; }
        string Summary { get; }

        INavigableItem NavigableItem { get; }
    }
}
