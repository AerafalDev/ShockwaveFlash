using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ShockwaveFlash.SourceGenerators.Avm1.Models;

namespace ShockwaveFlash.SourceGenerators.Avm1;

internal static class Avm1Parser
{
    internal static Avm1TypeModel? BuildTypeModel(INamedTypeSymbol symbol, Location location, CancellationToken cancellationToken, List<DiagnosticInfo> diagnostics, bool requirePartial)
    {
        if (symbol.IsGenericType || symbol.IsStatic || symbol.IsRefLikeType)
        {
            diagnostics.Add(Diag(Avm1Diagnostics.UnsupportedDeclaration, location, symbol.Name));
            return null;
        }

        if (requirePartial && !IsPartial(symbol, cancellationToken))
        {
            diagnostics.Add(Diag(Avm1Diagnostics.TypeMustBePartial, location, symbol.Name));
            return null;
        }

        var containingTypes = new List<string>();
        for (var outer = symbol.ContainingType; outer is not null; outer = outer.ContainingType)
        {
            if (requirePartial && !IsPartial(outer, cancellationToken))
            {
                diagnostics.Add(Diag(Avm1Diagnostics.ContainingTypeMustBePartial, location, symbol.Name, outer.Name));
                return null;
            }

            containingTypes.Insert(0, $"{TypeKeyword(outer)} {outer.Name}");
        }

        var construction = ResolveConstruction(symbol, out var constructorParameters);

        if (construction is null)
        {
            diagnostics.Add(Diag(Avm1Diagnostics.NoAccessibleConstructor, location, symbol.Name));
            return null;
        }

        var members = new List<Avm1MemberModel>();
        var keyOwners = new Dictionary<string, string>(StringComparer.Ordinal);
        var parameterNames = constructorParameters.Select(static p => p.Name).ToArray();
        var hasMemberDiagnostics = false;

        foreach (var member in symbol.GetMembers())
        {
            if (!TryDescribeMember(member, out var name, out var memberType, out var isSettable, out var memberLocation))
                continue;

            var isConstructorParameter = construction == ConstructionKind.Constructor
                && parameterNames.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase));

            if (!isConstructorParameter && !isSettable)
                continue;

            var key = GetPropertyKey(member) ?? name;
            if (keyOwners.TryGetValue(key, out var existing))
            {
                diagnostics.Add(Diag(Avm1Diagnostics.DuplicateMemberKey, memberLocation, symbol.Name, existing, name, key));
                hasMemberDiagnostics = true;
                continue;
            }

            if (!TryClassifyMember(memberType, out var classified))
            {
                diagnostics.Add(Diag(Avm1Diagnostics.UnsupportedMemberType, memberLocation, symbol.Name, name, memberType.ToDisplayString()));
                hasMemberDiagnostics = true;
                continue;
            }

            keyOwners[key] = name;

            var converterTypeFqn = GetConverterTypeFqn(member);
            if (converterTypeFqn is not null)
                classified = classified with
                {
                    ThrowIfMissing = !classified.MemberNullable,
                    Value = classified.Value with { Kind = ValueKind.Passthrough, IsValueType = false },
                };

            members.Add(classified with
            {
                CSharpName = name,
                Avm1Key = key,
                IsConstructorParameter = isConstructorParameter,
                IsSettable = isSettable,
                ConverterTypeFqn = converterTypeFqn,
            });
        }

        var constructorOrder = new List<string>();

        if (construction == ConstructionKind.Constructor)
        {
            foreach (var parameter in constructorParameters)
            {
                var match = members.FirstOrDefault(m => string.Equals(m.CSharpName, parameter.Name, StringComparison.OrdinalIgnoreCase));

                if (match.CSharpName is null)
                {
                    diagnostics.Add(Diag(Avm1Diagnostics.NoAccessibleConstructor, location, symbol.Name));
                    return null;
                }

                constructorOrder.Add(match.CSharpName);
            }
        }

        if (hasMemberDiagnostics)
            return null;

        return new Avm1TypeModel(
            symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToDisplayString(),
            new EquatableArray<string>(containingTypes.ToArray()),
            TypeKeyword(symbol),
            symbol.Name,
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            HintNameFor(symbol),
            GetGlobalName(symbol),
            construction.Value,
            new EquatableArray<string>(constructorOrder.ToArray()),
            new EquatableArray<Avm1MemberModel>(members.ToArray()));
    }

    internal static EquatableArray<DiagnosticInfo> ToArray(List<DiagnosticInfo> diagnostics)
    {
        return new(diagnostics.ToArray());
    }

    internal static DiagnosticInfo Diag(DiagnosticDescriptor descriptor, Location location, params string[] arguments)
    {
        return new(descriptor, LocationInfo.CreateFrom(location), new EquatableArray<string>(arguments));
    }


    internal static bool IsPartial(INamedTypeSymbol symbol, CancellationToken cancellationToken)
    {
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax(cancellationToken) is TypeDeclarationSyntax declaration && declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                return true;
        }

        return false;
    }

    internal static string TypeKeyword(INamedTypeSymbol symbol)
    {
        if (symbol.IsRecord)
            return symbol.IsValueType ? "record struct" : "record";

        return symbol.IsValueType ? "struct" : "class";
    }

    private static ConstructionKind? ResolveConstruction(INamedTypeSymbol symbol, out ImmutableArray<IParameterSymbol> constructorParameters)
    {
        var constructors = symbol.InstanceConstructors
            .Where(c => !c.IsStatic && IsAccessible(c.DeclaredAccessibility) && !IsCopyConstructor(symbol, c))
            .ToList();

        if (symbol.IsRecord)
        {
            var primary = constructors
                .Where(static c => c.Parameters.Length > 0)
                .OrderByDescending(static c => c.Parameters.Length)
                .FirstOrDefault();

            if (primary is not null)
            {
                constructorParameters = primary.Parameters;
                return ConstructionKind.Constructor;
            }
        }

        if (constructors.Any(c => c.Parameters.Length == 0) || symbol.IsValueType)
        {
            constructorParameters = ImmutableArray<IParameterSymbol>.Empty;
            return ConstructionKind.ObjectInitializer;
        }

        var parameterized = constructors.Where(static c => c.Parameters.Length > 0).ToList();
        if (parameterized.Count == 1)
        {
            constructorParameters = parameterized[0].Parameters;
            return ConstructionKind.Constructor;
        }

        constructorParameters = ImmutableArray<IParameterSymbol>.Empty;
        return null;
    }

    private static bool IsCopyConstructor(INamedTypeSymbol symbol, IMethodSymbol constructor)
    {
        return symbol.IsRecord
            && constructor.Parameters.Length == 1
            && SymbolEqualityComparer.Default.Equals(constructor.Parameters[0].Type, symbol);
    }


    private static bool TryDescribeMember(ISymbol member, out string name, out ITypeSymbol type, out bool isSettable, out Location location)
    {
        name = string.Empty;
        type = null!;
        isSettable = false;
        location = member.Locations.FirstOrDefault() ?? Location.None;

        if (member.IsStatic || member.IsImplicitlyDeclared || !IsAccessible(member.DeclaredAccessibility) || HasAttribute(member, Avm1SerializableGenerator.Avm1IgnoreAttributeName))
            return false;

        switch (member)
        {
            case IPropertySymbol { IsIndexer: false } property when IsAccessible(property.GetMethod?.DeclaredAccessibility):
                name = property.Name;
                type = property.Type;
                isSettable = property.SetMethod is { } setter && IsAccessible(setter.DeclaredAccessibility);
                return true;

            case IFieldSymbol { IsConst: false } field:
                name = field.Name;
                type = field.Type;
                isSettable = !field.IsReadOnly;
                return true;

            default:
                return false;
        }
    }

    private static bool TryClassifyMember(ITypeSymbol memberType, out Avm1MemberModel model)
    {
        model = default;

        var memberNullable = memberType.IsReferenceType && memberType.NullableAnnotation == NullableAnnotation.Annotated;
        var core = memberType;

        if (core is INamedTypeSymbol named && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            memberNullable = true;
            core = named.TypeArguments[0];
        }

        if (!TryBuildValueSpec(core, out var spec))
            return false;

        var declaredType = memberType
            .WithNullableAnnotation(NullableAnnotation.NotAnnotated)
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var throwIfMissing = !memberNullable && spec.Kind switch
        {
            ValueKind.Scalar => spec.Conv == ConvKind.String,
            ValueKind.Nested => true,
            ValueKind.Passthrough => true,
            _ => false,
        };

        model = new Avm1MemberModel(string.Empty, string.Empty, declaredType, spec, memberNullable, throwIfMissing, false, false, null);
        return true;
    }

    private static bool TryBuildValueSpec(ITypeSymbol type, out ValueSpec spec)
    {
        spec = null!;

        var core = type;
        if (core is INamedTypeSymbol nullable && nullable.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            core = nullable.TypeArguments[0];

        var fqn = core
            .WithNullableAnnotation(NullableAnnotation.NotAnnotated)
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        if (core.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString() is "ShockwaveFlash.Avm1.Types.Avm1Value" or "ShockwaveFlash.Avm1.Types.Avm1Object" or "ShockwaveFlash.Avm1.Types.Avm1Array")
        {
            spec = new ValueSpec(ValueKind.Passthrough, default, fqn, false, null);
            return true;
        }

        if (core.SpecialType == SpecialType.System_String)
        {
            spec = new ValueSpec(ValueKind.Scalar, ConvKind.String, fqn, false, null);
            return true;
        }

        if (core.SpecialType == SpecialType.System_Boolean)
        {
            spec = new ValueSpec(ValueKind.Scalar, ConvKind.Boolean, fqn, true, null);
            return true;
        }

        if (IsNumeric(core.SpecialType))
        {
            spec = new ValueSpec(ValueKind.Scalar, ConvKind.Number, fqn, true, null);
            return true;
        }

        if (core.TypeKind == TypeKind.Enum)
        {
            spec = new ValueSpec(ValueKind.Scalar, ConvKind.Enum, fqn, true, null);
            return true;
        }

        if (core is IArrayTypeSymbol { Rank: 1 } array && TryBuildValueSpec(array.ElementType, out var arrayElement))
        {
            spec = new ValueSpec(ValueKind.Array, default, arrayElement.TypeFqn, false, arrayElement);
            return true;
        }

        if (core is INamedTypeSymbol dictionary && TryGetDictionaryValue(dictionary, out var valueType) && TryBuildValueSpec(valueType, out var dictElement))
        {
            spec = new ValueSpec(ValueKind.Dictionary, default, dictElement.TypeFqn, false, dictElement);
            return true;
        }

        if (core is INamedTypeSymbol enumerable && TryGetEnumerableElement(enumerable, out var elementType) && TryBuildValueSpec(elementType, out var listElement))
        {
            spec = new ValueSpec(ValueKind.List, default, listElement.TypeFqn, false, listElement);
            return true;
        }

        if (core is INamedTypeSymbol)
        {
            spec = new ValueSpec(ValueKind.Nested, default, fqn, false, null);
            return true;
        }

        return false;
    }

    private static bool IsNumeric(SpecialType type)
    {
        return type
            is SpecialType.System_Byte
            or SpecialType.System_SByte
            or SpecialType.System_Int16
            or SpecialType.System_UInt16
            or SpecialType.System_Int32
            or SpecialType.System_UInt32
            or SpecialType.System_Int64
            or SpecialType.System_UInt64
            or SpecialType.System_Single
            or SpecialType.System_Double
            or SpecialType.System_Decimal;
    }


    private static bool TryGetDictionaryValue(INamedTypeSymbol type, out ITypeSymbol value)
    {
        if (MatchDictionary(type, out value))
            return true;

        foreach (var contract in type.AllInterfaces)
        {
            if (MatchDictionary(contract, out value))
                return true;
        }

        value = null!;
        return false;
    }

    private static bool MatchDictionary(INamedTypeSymbol candidate, out ITypeSymbol value)
    {
        if (candidate.IsGenericType
            && candidate.TypeArguments.Length == 2
            && candidate.TypeArguments[0].SpecialType == SpecialType.System_String
            && candidate.ConstructedFrom.ToDisplayString() is "System.Collections.Generic.IDictionary<TKey, TValue>" or "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>")
        {
            value = candidate.TypeArguments[1];
            return true;
        }

        value = null!;
        return false;
    }

    private static bool TryGetEnumerableElement(INamedTypeSymbol type, out ITypeSymbol element)
    {
        if (MatchEnumerable(type, out element))
            return true;

        foreach (var contract in type.AllInterfaces)
        {
            if (MatchEnumerable(contract, out element))
                return true;
        }

        element = null!;
        return false;
    }

    private static bool MatchEnumerable(INamedTypeSymbol candidate, out ITypeSymbol element)
    {
        if (candidate.IsGenericType
            && candidate.TypeArguments.Length == 1
            && candidate.ConstructedFrom.ToDisplayString() is "System.Collections.Generic.IEnumerable<T>")
        {
            element = candidate.TypeArguments[0];
            return true;
        }

        element = null!;
        return false;
    }

    private static bool HasAttribute(ISymbol symbol, string metadataName)
    {
        return symbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == metadataName);
    }


    private static string? GetPropertyKey(ISymbol symbol)
    {
        var attribute = symbol.GetAttributes().FirstOrDefault(static a => a.AttributeClass?.ToDisplayString() is Avm1SerializableGenerator.Avm1PropertyAttributeName);
        return attribute is { ConstructorArguments.Length: > 0 } ? attribute.ConstructorArguments[0].Value as string : null;
    }

    private static string? GetGlobalName(INamedTypeSymbol symbol)
    {
        var attribute = symbol.GetAttributes().FirstOrDefault(static a => a.AttributeClass?.ToDisplayString() is Avm1SerializableGenerator.Avm1ObjectAttributeName);
        return attribute is { ConstructorArguments.Length: > 0 } ? attribute.ConstructorArguments[0].Value as string : null;
    }

    private static string? GetConverterTypeFqn(ISymbol member)
    {
        var attribute = member.GetAttributes().FirstOrDefault(static a => a.AttributeClass?.ToDisplayString() is Avm1SerializableGenerator.Avm1ConverterAttributeName);
        return attribute is { ConstructorArguments.Length: > 0 } && attribute.ConstructorArguments[0].Value is INamedTypeSymbol converterType
            ? converterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            : null;
    }

    private static bool IsAccessible(Accessibility? accessibility)
    {
        return accessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal;
    }


    private static string HintNameFor(INamedTypeSymbol symbol)
    {
        var fullName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var builder = new StringBuilder(fullName.Length);

        foreach (var character in fullName)
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');

        return builder.Append(".Avm1.g.cs").ToString();
    }
}
