using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ShockwaveFlash.SourceGenerators.Avm1.Models;

namespace ShockwaveFlash.SourceGenerators.Avm1;

internal static class Avm1ContextParser
{
    public static Avm1ContextParseResult Parse(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var diagnostics = new List<DiagnosticInfo>();
        var symbol = (INamedTypeSymbol)context.TargetSymbol;
        var node = (TypeDeclarationSyntax)context.TargetNode;
        var location = node.Identifier.GetLocation();

        if (symbol.IsGenericType || symbol.IsStatic)
        {
            diagnostics.Add(Avm1Parser.Diag(Avm1Diagnostics.UnsupportedDeclaration, location, symbol.Name));
            return new Avm1ContextParseResult(null, Avm1Parser.ToArray(diagnostics));
        }

        if (!Avm1Parser.IsPartial(symbol, cancellationToken))
        {
            diagnostics.Add(Avm1Parser.Diag(Avm1Diagnostics.ContextMustBePartial, location, symbol.Name));
            return new Avm1ContextParseResult(null, Avm1Parser.ToArray(diagnostics));
        }

        var containingTypes = new List<string>();
        for (var outer = symbol.ContainingType; outer is not null; outer = outer.ContainingType)
        {
            if (!Avm1Parser.IsPartial(outer, cancellationToken))
            {
                diagnostics.Add(Avm1Parser.Diag(Avm1Diagnostics.ContainingTypeMustBePartial, location, symbol.Name, outer.Name));
                return new Avm1ContextParseResult(null, Avm1Parser.ToArray(diagnostics));
            }

            containingTypes.Insert(0, $"{Avm1Parser.TypeKeyword(outer)} {outer.Name}");
        }

        var registrations = new List<Avm1RegistrationModel>();
        var accessorNames = new HashSet<string>(StringComparer.Ordinal);
        var registeredSymbols = new List<INamedTypeSymbol>();
        var derivedRoots = new List<INamedTypeSymbol>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var noDerived = new EquatableArray<Avm1DerivedTypeModel>(Array.Empty<Avm1DerivedTypeModel>());

        foreach (var attribute in context.Attributes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (attribute.ConstructorArguments.Length == 0 || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol type)
                continue;

            var bindingPath = attribute.ConstructorArguments.Length > 1 ? attribute.ConstructorArguments[1].Value as string : null;
            var polymorphic = type.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == Avm1SerializableGenerator.Avm1PolymorphicAttributeName);

            if (polymorphic is not null)
            {
                var fqn = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var discriminatorName = NamedString(polymorphic, "TypeDiscriminatorPropertyName") ?? "$type";
                var derived = new List<Avm1DerivedTypeModel>();

                foreach (var derivedAttribute in type.GetAttributes().Where(a => a.AttributeClass?.ToDisplayString() == Avm1SerializableGenerator.Avm1DerivedTypeAttributeName))
                {
                    if (derivedAttribute.ConstructorArguments.Length >= 2
                        && derivedAttribute.ConstructorArguments[0].Value is INamedTypeSymbol derivedType
                        && derivedAttribute.ConstructorArguments[1].Value is string discriminator)
                    {
                        derived.Add(new Avm1DerivedTypeModel(derivedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), discriminator));
                        derivedRoots.Add(derivedType);
                    }
                }

                var polymorphicSegments = ResolveSegments(attribute, bindingPath, null);
                var polymorphicAccessor = UniqueAccessor(NamedString(attribute, "TypeInfoPropertyName") ?? type.Name, accessorNames);

                registrations.Add(new Avm1RegistrationModel(fqn, polymorphicAccessor, new EquatableArray<string>(polymorphicSegments), null, discriminatorName, new EquatableArray<Avm1DerivedTypeModel>(derived.ToArray())));
                seen.Add(fqn);
                continue;
            }

            var typeModel = Avm1Parser.BuildTypeModel(type, type.Locations.FirstOrDefault() ?? location, cancellationToken, diagnostics);
            if (typeModel is not { } model)
                continue;

            var segments = ResolveSegments(attribute, bindingPath, model.GlobalName);
            var accessor = UniqueAccessor(NamedString(attribute, "TypeInfoPropertyName") ?? model.TypeName, accessorNames);

            registrations.Add(new Avm1RegistrationModel(model.FullyQualifiedName, accessor, new EquatableArray<string>(segments), model, null, noDerived));
            registeredSymbols.Add(type);
            seen.Add(model.FullyQualifiedName);
        }

        foreach (var derivedType in derivedRoots)
        {
            var fqn = derivedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (!seen.Add(fqn))
                continue;

            if (Avm1Parser.BuildTypeModel(derivedType, derivedType.Locations.FirstOrDefault() ?? location, cancellationToken, diagnostics) is { } derivedModel)
            {
                registrations.Add(new Avm1RegistrationModel(derivedModel.FullyQualifiedName, UniqueAccessor(derivedModel.TypeName, accessorNames), new EquatableArray<string>(Array.Empty<string>()), derivedModel, null, noDerived));
                registeredSymbols.Add(derivedType);
            }
        }

        var nested = new List<INamedTypeSymbol>();
        foreach (var registered in registeredSymbols)
            CollectNested(registered, seen, nested, cancellationToken);

        foreach (var nestedType in nested)
        {
            var ignored = new List<DiagnosticInfo>();
            if (Avm1Parser.BuildTypeModel(nestedType, nestedType.Locations.FirstOrDefault() ?? location, cancellationToken, ignored) is { } nestedModel)
                registrations.Add(new Avm1RegistrationModel(nestedModel.FullyQualifiedName, UniqueAccessor(nestedModel.TypeName, accessorNames), new EquatableArray<string>(Array.Empty<string>()), nestedModel, null, noDerived));
        }

        var contextModel = new Avm1ContextModel(
            symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToDisplayString(),
            new EquatableArray<string>(containingTypes.ToArray()),
            symbol.Name,
            symbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal",
            HintNameFor(symbol),
            new EquatableArray<Avm1RegistrationModel>(registrations.ToArray()));

        return new Avm1ContextParseResult(contextModel, Avm1Parser.ToArray(diagnostics));
    }

    private static void CollectNested(INamedTypeSymbol root, HashSet<string> seen, List<INamedTypeSymbol> output, CancellationToken cancellationToken)
    {
        foreach (var member in root.GetMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();

            ITypeSymbol? memberType = member switch
            {
                IPropertySymbol { IsIndexer: false, IsStatic: false, GetMethod: not null } property => property.Type,
                IFieldSymbol { IsStatic: false, IsConst: false } field => field.Type,
                _ => null,
            };

            if (memberType is not null)
                Visit(memberType, seen, output, cancellationToken);
        }
    }

    private static void Visit(ITypeSymbol type, HashSet<string> seen, List<INamedTypeSymbol> output, CancellationToken cancellationToken)
    {
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
            type = nullable.TypeArguments[0];

        if (type is IArrayTypeSymbol array)
        {
            Visit(array.ElementType, seen, output, cancellationToken);
            return;
        }

        if (type is not INamedTypeSymbol named || Avm1Parser.IsLeaf(named))
            return;

        if (Avm1Parser.TryGetDictionaryValue(named, out var value))
        {
            Visit(value, seen, output, cancellationToken);
            return;
        }

        if (Avm1Parser.TryGetEnumerableElement(named, out var element))
        {
            Visit(element, seen, output, cancellationToken);
            return;
        }

        if (named.IsGenericType || named.IsStatic || Avm1Parser.HasAttribute(named, Avm1SerializableGenerator.Avm1ConverterAttributeName))
            return;

        var fqn = named.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (seen.Add(fqn))
        {
            output.Add(named);
            CollectNested(named, seen, output, cancellationToken);
        }
    }

    private static string[] ResolveSegments(AttributeData attribute, string? bindingPath, string? globalName)
    {
        var segments = NamedStringArray(attribute, "Segments");
        if (segments is { Length: > 0 })
            return segments;

        if (!string.IsNullOrEmpty(bindingPath))
            return bindingPath!.Split('.');

        if (!string.IsNullOrEmpty(globalName))
            return new[] { globalName! };

        return Array.Empty<string>();
    }

    private static string? NamedString(AttributeData attribute, string name)
    {
        foreach (var pair in attribute.NamedArguments)
        {
            if (pair.Key == name)
                return pair.Value.Value as string;
        }

        return null;
    }

    private static string[]? NamedStringArray(AttributeData attribute, string name)
    {
        foreach (var pair in attribute.NamedArguments)
        {
            if (pair.Key != name || pair.Value.IsNull)
                continue;

            return pair.Value.Values
                .Select(static v => v.Value as string)
                .Where(static v => !string.IsNullOrEmpty(v))
                .Select(static v => v!)
                .ToArray();
        }

        return null;
    }

    private static string UniqueAccessor(string preferred, HashSet<string> taken)
    {
        var name = Sanitize(preferred);

        if (taken.Add(name))
            return name;

        for (var index = 2; ; index++)
        {
            var candidate = name + index.ToString(CultureInfo.InvariantCulture);
            if (taken.Add(candidate))
                return candidate;
        }
    }

    private static string Sanitize(string name)
    {
        var builder = new StringBuilder(name.Length);

        foreach (var character in name)
            builder.Append(char.IsLetterOrDigit(character) || character == '_' ? character : '_');

        return builder.Length == 0 ? "Type" : builder.ToString();
    }

    private static string HintNameFor(INamedTypeSymbol symbol)
    {
        var fullName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var builder = new StringBuilder(fullName.Length);

        foreach (var character in fullName)
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');

        return builder.Append(".Avm1Context.g.cs").ToString();
    }
}
