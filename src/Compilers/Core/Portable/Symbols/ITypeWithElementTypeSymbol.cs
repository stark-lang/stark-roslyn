﻿namespace Microsoft.CodeAnalysis
{
    public interface ITypeWithElementTypeSymbol : ITypeSymbol
    {
        ITypeSymbol ElementType { get; }
    }
}
