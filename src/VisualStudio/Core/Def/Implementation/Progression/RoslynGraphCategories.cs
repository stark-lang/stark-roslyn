﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using StarkPlatform.CodeAnalysis;
using StarkPlatform.CodeAnalysis.Text;
using Microsoft.VisualStudio.GraphModel;

namespace StarkPlatform.VisualStudio.LanguageServices.Implementation.Progression
{
    internal static class RoslynGraphCategories
    {
        public static readonly GraphSchema Schema;

        public static readonly GraphCategory Overrides;

        static RoslynGraphCategories()
        {
            Schema = RoslynGraphProperties.Schema;

            Overrides = Schema.Categories.AddNewCategory(
                "Overrides",
                () => new GraphMetadata(GraphMetadataOptions.Sharable));
        }
    }
}
