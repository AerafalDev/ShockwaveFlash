using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ShockwaveFlash.SourceGenerators.Avm1.Models;

namespace ShockwaveFlash.SourceGenerators.Avm1;

[Generator(LanguageNames.CSharp)]
public sealed class Avm1SerializableGenerator : IIncrementalGenerator
{
    internal const string Avm1ObjectAttributeName = "ShockwaveFlash.Avm1.Serialization.Avm1ObjectAttribute";
    internal const string Avm1PropertyAttributeName = "ShockwaveFlash.Avm1.Serialization.Avm1PropertyAttribute";
    internal const string Avm1IgnoreAttributeName = "ShockwaveFlash.Avm1.Serialization.Avm1IgnoreAttribute";
    internal const string Avm1IncludeAttributeName = "ShockwaveFlash.Avm1.Serialization.Avm1IncludeAttribute";
    internal const string Avm1ConverterAttributeName = "ShockwaveFlash.Avm1.Serialization.Avm1ConverterAttribute";
    internal const string Avm1RequiredAttributeName = "ShockwaveFlash.Avm1.Serialization.Avm1RequiredAttribute";
    internal const string Avm1ConstructorAttributeName = "ShockwaveFlash.Avm1.Serialization.Avm1ConstructorAttribute";
    internal const string Avm1PropertyOrderAttributeName = "ShockwaveFlash.Avm1.Serialization.Avm1PropertyOrderAttribute";
    internal const string Avm1ExtensionDataAttributeName = "ShockwaveFlash.Avm1.Serialization.Avm1ExtensionDataAttribute";
    internal const string Avm1PolymorphicAttributeName = "ShockwaveFlash.Avm1.Serialization.Avm1PolymorphicAttribute";
    internal const string Avm1DerivedTypeAttributeName = "ShockwaveFlash.Avm1.Serialization.Avm1DerivedTypeAttribute";
    internal const string Avm1SerializableAttributeName = "ShockwaveFlash.Avm1.Serialization.Avm1SerializableAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var contexts = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Avm1SerializableAttributeName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) => Avm1ContextParser.Parse(ctx, ct))
            .WithTrackingName(TrackingNames.ParseContext);

        context.RegisterSourceOutput(contexts, static (productionContext, result) =>
        {
            foreach (var diagnostic in result.Diagnostics)
                productionContext.ReportDiagnostic(diagnostic.ToDiagnostic());

            if (result.Model is { } model)
                productionContext.AddSource(model.HintName, SourceText.From(Avm1ContextEmitter.Emit(model), Encoding.UTF8));
        });
    }
}
