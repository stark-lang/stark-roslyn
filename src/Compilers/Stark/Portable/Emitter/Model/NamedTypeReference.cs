﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Stark.Metadata;
using StarkPlatform.CodeAnalysis.Stark.Symbols;
using StarkPlatform.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace StarkPlatform.CodeAnalysis.Stark.Emit
{
    internal abstract class NamedTypeReference : Cci.INamedTypeReference
    {
        protected readonly NamedTypeSymbol UnderlyingNamedType;

        public NamedTypeReference(NamedTypeSymbol underlyingNamedType)
        {
            Debug.Assert((object)underlyingNamedType != null);

            this.UnderlyingNamedType = underlyingNamedType;
        }

        ushort Cci.INamedTypeReference.GenericParameterCount
        {
            get
            {
                return (ushort)UnderlyingNamedType.Arity;
            }
        }

        bool Cci.INamedTypeReference.MangleName
        {
            get
            {
                return UnderlyingNamedType.MangleName;
            }
        }

        string Cci.INamedEntity.Name
        {
            get
            {
                return UnderlyingNamedType.MetadataName;
            }
        }

        bool Cci.ITypeReference.IsEnum
        {
            get
            {
                return UnderlyingNamedType.IsEnumType();
            }
        }

        bool Cci.ITypeReference.IsValueType
        {
            get
            {
                return UnderlyingNamedType.IsValueType;
            }
        }

        Cci.ITypeDefinition Cci.ITypeReference.GetResolvedType(EmitContext context)
        {
            return null;
        }

        Cci.PrimitiveTypeCode Cci.ITypeReference.TypeCode
        {
            get
            {
                return Cci.PrimitiveTypeCode.NotPrimitive;
            }
        }

        TypeDefinitionHandle Cci.ITypeReference.TypeDef
        {
            get
            {
                return default(TypeDefinitionHandle);
            }
        }

        Cci.IGenericMethodParameterReference Cci.ITypeReference.AsGenericMethodParameterReference
        {
            get
            {
                return null;
            }
        }

        public abstract Cci.IGenericTypeInstanceReference AsGenericTypeInstanceReference
        {
            get;
        }

        Cci.IGenericTypeParameterReference Cci.ITypeReference.AsGenericTypeParameterReference
        {
            get
            {
                return null;
            }
        }

        Cci.INamespaceTypeDefinition Cci.ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
        {
            return null;
        }

        public abstract Cci.INamespaceTypeReference AsNamespaceTypeReference
        {
            get;
        }

        Cci.INestedTypeDefinition Cci.ITypeReference.AsNestedTypeDefinition(EmitContext context)
        {
            return null;
        }

        public abstract Cci.INestedTypeReference AsNestedTypeReference
        {
            get;
        }

        public abstract Cci.ISpecializedNestedTypeReference AsSpecializedNestedTypeReference
        {
            get;
        }

        Cci.ITypeDefinition Cci.ITypeReference.AsTypeDefinition(EmitContext context)
        {
            return null;
        }

        public override string ToString()
        {
            return UnderlyingNamedType.ToDisplayString(SymbolDisplayFormat.ILVisualizationFormat);
        }

        IEnumerable<Cci.ICustomAttribute> Cci.IReference.GetAttributes(EmitContext context)
        {
            return SpecializedCollections.EmptyEnumerable<Cci.ICustomAttribute>();
        }

        public abstract void Dispatch(Cci.MetadataVisitor visitor);

        Cci.IDefinition Cci.IReference.AsDefinition(EmitContext context)
        {
            return null;
        }

        public TypeAccessModifiers AccessModifiers => UnderlyingNamedType.AccessModifiers;
    }
}
