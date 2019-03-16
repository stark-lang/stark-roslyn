﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using StarkPlatform.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace StarkPlatform.CodeAnalysis.Stark.Symbols
{
    /// <summary>
    /// Represents a type parameter in a generic type or generic method.
    /// </summary>
    internal abstract partial class TypeParameterSymbol : TypeSymbol, ITypeParameterSymbol
    {
        /// <summary>
        /// The original definition of this symbol. If this symbol is constructed from another
        /// symbol by type substitution then OriginalDefinition gets the original symbol as it was defined in
        /// source or metadata.
        /// </summary>
        public new virtual TypeParameterSymbol OriginalDefinition
        {
            get
            {
                return this;
            }
        }

        protected override sealed TypeSymbol OriginalTypeSymbolDefinition
        {
            get
            {
                return this.OriginalDefinition;
            }
        }

        /// <summary>
        /// If this is a type parameter of a reduced extension method, gets the type parameter definition that
        /// this type parameter was reduced from. Otherwise, returns Nothing.
        /// </summary>
        public virtual TypeParameterSymbol ReducedFrom
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The ordinal position of the type parameter in the parameter list which declares
        /// it. The first type parameter has ordinal zero.
        /// </summary>
        public abstract int Ordinal
        {
            // This is needed to determine hiding in C#: 
            //
            // interface IB { void M<T>(C<T> x); }
            // interface ID : IB { new void M<U>(C<U> x); }
            //
            // ID.M<U> hides IB.M<T> even though their formal parameters have different
            // types. When comparing formal parameter types for hiding purposes we must
            // compare method type parameters by ordinal, not by identity.
            get;
        }

        internal virtual DiagnosticInfo GetConstraintsUseSiteErrorInfo()
        {
            return null;
        }

        /// <summary>
        /// The types that were directly specified as constraints on the type parameter.
        /// Duplicates and cycles are removed, although the collection may include
        /// redundant constraints where one constraint is a base type of another.
        /// </summary>
        internal ImmutableArray<TypeSymbolWithAnnotations> GetConstraintTypesNoUseSiteDiagnostics(ConsList<TypeParameterSymbol> inProgress, bool early)
        {
            // We could call EnsureAllConstraintsAreResolved(early) directly rather than splitting
            // this into two separate calls. However, the split between early and late must be explicit
            // somewhere to ensure that the early work for all sibling type parameters is completed
            // before the late work for any sibling. It seems simpler to handle the split here (in the
            // one caller of EnsureAllConstraintsAreResolved that supports the late phase) rather than
            // in several implementations of the abstract EnsureAllConstraintsAreResolved method.
            this.EnsureAllConstraintsAreResolved(early: true);
            if (!early)
            {
                this.EnsureAllConstraintsAreResolved(early);
            }
            return this.GetConstraintTypes(inProgress, early);
        }

        internal ImmutableArray<TypeSymbolWithAnnotations> ConstraintTypesNoUseSiteDiagnostics =>
            GetConstraintTypesNoUseSiteDiagnostics(ConsList<TypeParameterSymbol>.Empty, early: false);

        internal ImmutableArray<TypeSymbolWithAnnotations> ConstraintTypesWithDefinitionUseSiteDiagnostics(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
        {
            var result = ConstraintTypesNoUseSiteDiagnostics;

            AppendConstraintsUseSiteErrorInfo(ref useSiteDiagnostics);

            foreach (var constraint in result)
            {
                ((TypeSymbol)constraint.TypeSymbol.OriginalDefinition).AddUseSiteDiagnostics(ref useSiteDiagnostics);
            }

            return result;
        }

        private void AppendConstraintsUseSiteErrorInfo(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
        {
            DiagnosticInfo errorInfo = this.GetConstraintsUseSiteErrorInfo();

            if ((object)errorInfo != null)
            {
                if (useSiteDiagnostics == null)
                {
                    useSiteDiagnostics = new HashSet<DiagnosticInfo>();
                }

                useSiteDiagnostics.Add(errorInfo);
            }
        }

        /// <summary>
        /// True if the parameterless constructor constraint was specified for the type parameter.
        /// </summary>
        public abstract bool HasConstructorConstraint { get; }

        /// <summary>
        /// The type parameter kind of this type parameter.
        /// </summary>
        public abstract TypeParameterKind TypeParameterKind { get; }

        /// <summary>
        /// The method that declared this type parameter, or null.
        /// </summary>
        public MethodSymbol DeclaringMethod
        {
            get
            {
                return this.ContainingSymbol as MethodSymbol;
            }
        }

        /// <summary>
        /// The type that declared this type parameter, or null.
        /// </summary>
        public NamedTypeSymbol DeclaringType
        {
            get
            {
                return this.ContainingSymbol as NamedTypeSymbol;
            }
        }

        // Type parameters do not have members
        public sealed override ImmutableArray<Symbol> GetMembers()
        {
            return ImmutableArray<Symbol>.Empty;
        }

        // Type parameters do not have members
        public sealed override ImmutableArray<Symbol> GetMembers(string name)
        {
            return ImmutableArray<Symbol>.Empty;
        }

        // Type parameters do not have members
        public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
        {
            return ImmutableArray<NamedTypeSymbol>.Empty;
        }

        // Type parameters do not have members
        public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
        {
            return ImmutableArray<NamedTypeSymbol>.Empty;
        }

        // Type parameters do not have members
        public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
        {
            return ImmutableArray<NamedTypeSymbol>.Empty;
        }

        internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitTypeParameter(this, argument);
        }

        public override void Accept(CSharpSymbolVisitor visitor)
        {
            visitor.VisitTypeParameter(this);
        }

        public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
        {
            return visitor.VisitTypeParameter(this);
        }

        public sealed override SymbolKind Kind
        {
            get
            {
                return SymbolKind.TypeParameter;
            }
        }

        public sealed override TypeKind TypeKind
        {
            get
            {
                return TypeKind.TypeParameter;
            }
        }

        // Only the compiler can create TypeParameterSymbols.
        internal TypeParameterSymbol()
        {
        }

        public sealed override Accessibility DeclaredAccessibility
        {
            get
            {
                return Accessibility.NotApplicable;
            }
        }

        public sealed override bool IsStatic
        {
            get
            {
                return false;
            }
        }

        public sealed override bool IsAbstract
        {
            get
            {
                return false;
            }
        }

        public sealed override bool IsSealed
        {
            get
            {
                return false;
            }
        }

        internal sealed override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => null;

        internal sealed override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol> basesBeingResolved = null)
        {
            return ImmutableArray<NamedTypeSymbol>.Empty;
        }

        protected override ImmutableArray<NamedTypeSymbol> GetAllInterfaces()
        {
            return ImmutableArray<NamedTypeSymbol>.Empty;
        }

        /// <summary>
        /// The effective base class of the type parameter (spec 10.1.5). If the deduced
        /// base type is a reference type, the effective base type will be the same as
        /// the deduced base type. Otherwise if the deduced base type is a value type,
        /// the effective base type will be the most derived reference type from which
        /// deduced base type is derived.
        /// </summary>
        internal NamedTypeSymbol EffectiveBaseClassNoUseSiteDiagnostics
        {
            get
            {
                this.EnsureAllConstraintsAreResolved(early: false);
                return this.GetEffectiveBaseClass(ConsList<TypeParameterSymbol>.Empty);
            }
        }

        internal NamedTypeSymbol EffectiveBaseClass(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
        {
            AppendConstraintsUseSiteErrorInfo(ref useSiteDiagnostics);
            var result = EffectiveBaseClassNoUseSiteDiagnostics;

            if ((object)result != null)
            {
                result.OriginalDefinition.AddUseSiteDiagnostics(ref useSiteDiagnostics);
            }

            return result;
        }

        /// <summary>
        /// The effective interface set (spec 10.1.5).
        /// </summary>
        internal ImmutableArray<NamedTypeSymbol> EffectiveInterfacesNoUseSiteDiagnostics
        {
            get
            {
                this.EnsureAllConstraintsAreResolved(early: false);
                return this.GetInterfaces(ConsList<TypeParameterSymbol>.Empty);
            }
        }

        /// <summary>
        /// The most encompassed type (spec 6.4.2) from the constraints.
        /// </summary>
        internal TypeSymbol DeducedBaseTypeNoUseSiteDiagnostics
        {
            get
            {
                this.EnsureAllConstraintsAreResolved(early: false);
                return this.GetDeducedBaseType(ConsList<TypeParameterSymbol>.Empty);
            }
        }

        internal TypeSymbol DeducedBaseType(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
        {
            AppendConstraintsUseSiteErrorInfo(ref useSiteDiagnostics);
            var result = DeducedBaseTypeNoUseSiteDiagnostics;

            if ((object)result != null)
            {
                ((TypeSymbol)result.OriginalDefinition).AddUseSiteDiagnostics(ref useSiteDiagnostics);
            }

            return result;
        }

        /// <summary>
        /// The effective interface set and any base interfaces of those
        /// interfaces. This is AllInterfaces excluding interfaces that are
        /// only implemented by the effective base type.
        /// </summary>
        internal ImmutableArray<NamedTypeSymbol> AllEffectiveInterfacesNoUseSiteDiagnostics
        {
            get
            {
                return base.GetAllInterfaces();
            }
        }

        internal ImmutableArray<NamedTypeSymbol> AllEffectiveInterfacesWithDefinitionUseSiteDiagnostics(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
        {
            var result = AllEffectiveInterfacesNoUseSiteDiagnostics;

            // Since bases affect content of AllInterfaces set, we need to make sure they all are good.
            var current = DeducedBaseType(ref useSiteDiagnostics);

            while ((object)current != null)
            {
                current = current.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
            }

            foreach (var iface in result)
            {
                iface.OriginalDefinition.AddUseSiteDiagnostics(ref useSiteDiagnostics);
            }

            return result;
        }

        /// <summary>
        /// Called by <see cref="ConstraintTypesNoUseSiteDiagnostics"/>, <see cref="InterfacesNoUseSiteDiagnostics"/>, <see cref="EffectiveBaseClass"/>, and <see cref="DeducedBaseType"/>.
        /// to allow derived classes to ensure constraints within the containing
        /// type or method are resolved in a consistent order, regardless of the
        /// order the callers query individual type parameters.
        /// The `early` parameter indicates whether the constraints can be from
        /// the initial phase of constraint checking where the constraint types may
        /// still contain invalid or duplicate types.
        /// </summary>
        internal abstract void EnsureAllConstraintsAreResolved(bool early);

        /// <summary>
        /// Helper method to force type parameter constraints to be resolved.
        /// </summary>
        protected static void EnsureAllConstraintsAreResolved(ImmutableArray<TypeParameterSymbol> typeParameters, bool early)
        {
            foreach (var typeParameter in typeParameters)
            {
                // Invoke any method that forces constraints to be resolved.
                var unused = typeParameter.GetConstraintTypes(ConsList<TypeParameterSymbol>.Empty, early);
            }
        }

        internal abstract ImmutableArray<TypeSymbolWithAnnotations> GetConstraintTypes(ConsList<TypeParameterSymbol> inProgress, bool early);

        internal abstract ImmutableArray<NamedTypeSymbol> GetInterfaces(ConsList<TypeParameterSymbol> inProgress);

        internal abstract NamedTypeSymbol GetEffectiveBaseClass(ConsList<TypeParameterSymbol> inProgress);

        internal abstract TypeSymbol GetDeducedBaseType(ConsList<TypeParameterSymbol> inProgress);

        private bool ConstraintImpliesReferenceType(TypeSymbol constraint, ConsList<TypeParameterSymbol> inProgress)
        {
            if (constraint.TypeKind == TypeKind.TypeParameter)
            {
                var typeParameter = ((TypeParameterSymbol)constraint);

                if (inProgress.ContainsReference(typeParameter))
                {
                    return false;
                }

                var constraints = typeParameter.GetConstraintTypesNoUseSiteDiagnostics(inProgress, early: true);
                return IsReferenceTypeFromConstraintTypes(constraints, inProgress);
            }
            else if (!constraint.IsReferenceType)
            {
                return false;
            }
            else
            {
                switch (constraint.TypeKind)
                {
                    case TypeKind.Interface:
                        return false; // can be satisfied by value types
                    case TypeKind.Error:
                        return false;
                }

                switch (constraint.SpecialType)
                {
                    case SpecialType.System_Object:
                    case SpecialType.System_Enum:
                        return false; // can be satisfied by value types
                }

                return true;
            }
        }

        // From typedesc.cpp :
        // > A recursive helper that helps determine whether this variable is constrained as ObjRef.
        // > Please note that we do not check the gpReferenceTypeConstraint special constraint here
        // > because this property does not propagate up the constraining hierarchy.
        // > (e.g. "class A<S, T> where S : T, where T : class" does not guarantee that S is ObjRef)
        internal bool IsReferenceTypeFromConstraintTypes(ImmutableArray<TypeSymbolWithAnnotations> constraintTypes, ConsList<TypeParameterSymbol> inProgress)
        {
            return AnyConstraintTypes(constraintTypes, inProgress, (type, arg) => ConstraintImpliesReferenceType(type.TypeSymbol, arg));
        }

        internal static bool? IsNotNullableIfReferenceTypeFromConstraintTypes(ImmutableArray<TypeSymbolWithAnnotations> constraintTypes)
        {
            Debug.Assert(!constraintTypes.IsDefaultOrEmpty);

            bool? result = false;
            foreach (TypeSymbolWithAnnotations constraintType in constraintTypes)
            {
                bool? fromType = IsNotNullableIfReferenceTypeFromConstraintType(constraintType);
                if (fromType == true)
                {
                    return true;
                }
                else if (fromType == null)
                {
                    result = null;
                }
            }

            return result;
        }

        internal static bool? IsNotNullableIfReferenceTypeFromConstraintType(TypeSymbolWithAnnotations constraintType)
        {
            if (constraintType.NullableAnnotation.IsAnyNullable())
            {
                return false;
            }

            if (constraintType.TypeKind == TypeKind.TypeParameter)
            {
                bool? isNotNullableIfReferenceType = ((TypeParameterSymbol)constraintType.TypeSymbol).GetIsNotNullableIfReferenceType();

                if (isNotNullableIfReferenceType == false)
                {
                    return false;
                }
                else if (isNotNullableIfReferenceType == null)
                {
                    return null;
                }
            }

            if (constraintType.NullableAnnotation == NullableAnnotation.Unknown)
            {
                return null;
            }

            return true;
        }

        internal bool IsValueTypeFromConstraintTypes(ImmutableArray<TypeSymbolWithAnnotations> constraintTypes, ConsList<TypeParameterSymbol> inProgress)
        {
            return AnyConstraintTypes(constraintTypes, inProgress, (type, arg) => type.GetIsValueType(arg));
        }

        private bool AnyConstraintTypes(
            ImmutableArray<TypeSymbolWithAnnotations> constraintTypes,
            ConsList<TypeParameterSymbol> inProgress,
            Func<TypeSymbolWithAnnotations, ConsList<TypeParameterSymbol>, bool> predicate)
        {
            if (constraintTypes.IsEmpty)
            {
                return false;
            }
            inProgress = inProgress.Prepend(this);
            foreach (var constraintType in constraintTypes)
            {
                if (predicate(constraintType, inProgress))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool GetIsReferenceType(ConsList<TypeParameterSymbol> inProgress)
        {
            if (inProgress.ContainsReference(this))
            {
                return false;
            }
            if (this.HasReferenceTypeConstraint)
            {
                return true;
            }
            return IsReferenceTypeFromConstraintTypes(this.GetConstraintTypesNoUseSiteDiagnostics(inProgress, early: true), inProgress);
        }

        public sealed override bool IsReferenceType => GetIsReferenceType(ConsList<TypeParameterSymbol>.Empty);

        internal bool? GetIsNotNullableIfReferenceType()
        {
            bool? fromReferenceTypeConstraint = false;

            if (this.HasReferenceTypeConstraint)
            {
                fromReferenceTypeConstraint = !this.ReferenceTypeConstraintIsNullable;

                if (fromReferenceTypeConstraint == true)
                {
                    return true;
                }
            }

            ImmutableArray<TypeSymbolWithAnnotations> constraintTypes = this.ConstraintTypesNoUseSiteDiagnostics;

            if (constraintTypes.IsEmpty)
            {
                return fromReferenceTypeConstraint;
            }

            bool? fromTypes = IsNotNullableIfReferenceTypeFromConstraintTypes(constraintTypes);

            if (fromTypes == true || fromReferenceTypeConstraint == false)
            {
                return fromTypes;
            }

            Debug.Assert(fromReferenceTypeConstraint == null);
            Debug.Assert(fromTypes != true);
            return null;
        }

        // https://github.com/dotnet/roslyn/issues/26198 Should this API be exposed through ITypeParameterSymbol?
        internal bool? IsNotNullableIfReferenceType => GetIsNotNullableIfReferenceType();

        internal bool GetIsValueType(ConsList<TypeParameterSymbol> inProgress)
        {
            if (inProgress.ContainsReference(this))
            {
                return false;
            }
            if (this.HasValueTypeConstraint)
            {
                return true;
            }
            return IsValueTypeFromConstraintTypes(this.GetConstraintTypesNoUseSiteDiagnostics(inProgress, early: true), inProgress);
        }

        public sealed override bool IsValueType => GetIsValueType(ConsList<TypeParameterSymbol>.Empty);
        
        internal sealed override ManagedKind ManagedKind
        {
            get
            {
                return HasUnmanagedTypeConstraint ? ManagedKind.Unmanaged : ManagedKind.Managed;
            }
        }

        public sealed override bool IsRefLikeType
        {
            get
            {
                return false;
            }
        }

        internal sealed override bool IsReadOnly
        {
            get
            {
                // even if T is indirectly constrained to a struct, 
                // we only can use members via constrained calls, so "true" would have no effect
                return false;
            }
        }

        internal sealed override ObsoleteAttributeData ObsoleteAttributeData
        {
            get { return null; }
        }

        public abstract bool HasReferenceTypeConstraint { get; }

        /// <summary>
        /// Returns whether the reference type constraint (the 'class' constraint) should also be treated as nullable ('class?') or non-nullable (class!).
        /// In some cases this aspect is unknown (null value is returned). For example, when 'class' constraint is specified in a NonNullTypes(false) context.  
        /// This API returns false when <see cref="HasReferenceTypeConstraint"/> is false.
        /// </summary>
        internal abstract bool? ReferenceTypeConstraintIsNullable { get; }

        public abstract bool HasValueTypeConstraint { get; }

        public abstract bool HasConstTypeConstraint { get; }

        public abstract bool HasUnmanagedTypeConstraint { get; }

        public abstract VarianceKind Variance { get; }

        internal sealed override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
        {
            return false;
        }

        internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
        {
            return this.Equals(t2 as TypeParameterSymbol, comparison);
        }

        internal bool Equals(TypeParameterSymbol other)
        {
            return Equals(other, TypeCompareKind.ConsiderEverything);
        }

        private bool Equals(TypeParameterSymbol other, TypeCompareKind comparison)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if ((object)other == null || !ReferenceEquals(other.OriginalDefinition, this.OriginalDefinition))
            {
                return false;
            }

            // Type parameters may be equal but not reference equal due to independent alpha renamings.
            return other.ContainingSymbol.ContainingType.Equals(this.ContainingSymbol.ContainingType, comparison);
        }

        public override int GetHashCode()
        {
            return Hash.Combine(ContainingSymbol, Ordinal);
        }

        internal override void AddNullableTransforms(ArrayBuilder<byte> transforms)
        {
        }

        internal override bool ApplyNullableTransforms(byte defaultTransformFlag, ImmutableArray<byte> transforms, ref int position, out TypeSymbol result)
        {
            result = this;
            return true;
        }

        internal override TypeSymbol SetNullabilityForReferenceTypes(Func<TypeSymbolWithAnnotations, TypeSymbolWithAnnotations> transform)
        {
            return this;
        }

        internal override TypeSymbol MergeNullability(TypeSymbol other, VarianceKind variance, out bool hadNullabilityMismatch)
        {
            Debug.Assert(this.Equals(other, TypeCompareKind.IgnoreDynamicAndTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes));
            hadNullabilityMismatch = false;
            return this;
        }

        #region ITypeParameterTypeSymbol Members

        TypeParameterKind ITypeParameterSymbol.TypeParameterKind
        {
            get
            {
                return (TypeParameterKind)this.TypeParameterKind;
            }
        }

        IMethodSymbol ITypeParameterSymbol.DeclaringMethod
        {
            get { return this.DeclaringMethod; }
        }

        INamedTypeSymbol ITypeParameterSymbol.DeclaringType
        {
            get { return this.DeclaringType; }
        }

        ImmutableArray<ITypeSymbol> ITypeParameterSymbol.ConstraintTypes
        {
            get
            {
                return this.ConstraintTypesNoUseSiteDiagnostics.SelectAsArray(c => (ITypeSymbol)c.TypeSymbol);
            }
        }

        ITypeParameterSymbol ITypeParameterSymbol.OriginalDefinition
        {
            get { return this.OriginalDefinition; }
        }

        ITypeParameterSymbol ITypeParameterSymbol.ReducedFrom
        {
            get { return this.ReducedFrom; }
        }

        #endregion

        #region ISymbol Members

        public override void Accept(SymbolVisitor visitor)
        {
            visitor.VisitTypeParameter(this);
        }

        public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
        {
            return visitor.VisitTypeParameter(this);
        }

        #endregion
    }
}
