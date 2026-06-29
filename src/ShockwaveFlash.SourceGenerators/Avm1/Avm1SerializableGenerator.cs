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
    internal const string Avm1SerializableAttributeName = "ShockwaveFlash.Avm1.Serialization.Avm1SerializableAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var results = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Avm1ObjectAttributeName,
                predicate: static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax or StructDeclarationSyntax,
                transform: static (ctx, ct) => Avm1Parser.Parse(ctx, ct))
            .WithTrackingName(TrackingNames.Parse);

        context.RegisterSourceOutput(results, static (productionContext, result) =>
        {
            foreach (var diagnostic in result.Diagnostics)
                productionContext.ReportDiagnostic(diagnostic.ToDiagnostic());

            if (result.Model is { } model)
                productionContext.AddSource(model.HintName, SourceText.From(Avm1SerializableEmitter.Emit(model), Encoding.UTF8));
        });

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
