﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;

namespace StarkPlatform.CodeAnalysis.Stark.Syntax
{
    public sealed partial class ArgumentSyntax
    {
        /// <summary>
        /// Pre C# 7.2 back-compat overload, which simply calls the replacement property <see cref="RefKindKeyword"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public SyntaxToken RefOrOutKeyword
        {
            get => this.RefKindKeyword;
        }

        /// <summary>
        /// Pre C# 7.2 back-compat overload, which simply calls the replacement method <see cref="Update"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ArgumentSyntax WithRefOrOutKeyword(SyntaxToken refOrOutKeyword)
        {
            return this.Update(this.NameColon, refOrOutKeyword, this.Expression);
        }
    }
}
