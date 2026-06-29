using System.Collections.Immutable;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.SourceGenerators.Avm1;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class Avm1GeneratorDriverTests
{
    private const string ValidSource = """
        using ShockwaveFlash.Avm1.Serialization;

        namespace Demo;

        [Avm1Object("d")]
        public partial record Item([property: Avm1Property("n")] string Name, int Count);
        """;

    [Fact]
    public void Generates_a_serializer_for_a_valid_type()
    {
        var (result, diagnostics) = Run(ValidSource);

        diagnostics.ShouldBeEmpty();

        var generated = result.GeneratedSources.Single(s => s.HintName.EndsWith(".Avm1.g.cs", StringComparison.Ordinal));
        var text = generated.SourceText.ToString();

        text.ShouldContain("ToAvm1Object()");
        text.ShouldContain("FromAvm1Object(");
        text.ShouldContain("Avm1GlobalName");
    }

    [Fact]
    public void Reports_AVM1001_for_a_non_partial_type()
    {
        var (_, diagnostics) = Run("""
            using ShockwaveFlash.Avm1.Serialization;
            [Avm1Object]
            public record NotPartial(string Name);
            """);

        diagnostics.Select(d => d.Id).ShouldContain("AVM1001");
    }

    [Fact]
    public void Reports_AVM1002_for_an_unsupported_member()
    {
        var (_, diagnostics) = Run("""
            using ShockwaveFlash.Avm1.Serialization;
            [Avm1Object]
            public partial record Bag(object Payload);
            """);

        diagnostics.Select(d => d.Id).ShouldContain("AVM1002");
    }

    [Fact]
    public void Pipeline_output_is_cached_on_an_unrelated_edit()
    {
        var compilation = Compile(ValidSource);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new Avm1SerializableGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation);

        var edited = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("namespace Other { class Extra { } }"));
        driver = driver.RunGenerators(edited);

        var tracked = driver.GetRunResult().Results[0].TrackedSteps;
        tracked.ShouldContainKey("Avm1Parse");
        tracked["Avm1Parse"]
            .SelectMany(step => step.Outputs)
            .ShouldAllBe(output => output.Reason == IncrementalStepRunReason.Cached || output.Reason == IncrementalStepRunReason.Unchanged);
    }

    private static (GeneratorRunResult Result, ImmutableArray<Diagnostic> Diagnostics) Run(string source)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new Avm1SerializableGenerator().AsSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(Compile(source), out _, out var diagnostics);
        return (driver.GetRunResult().Results[0], diagnostics);
    }

    private static CSharpCompilation Compile(string source)
    {
        var references = new List<MetadataReference>(Net100.References.All)
        {
            MetadataReference.CreateFromFile(typeof(Avm1ObjectAttribute).Assembly.Location),
        };

        return CSharpCompilation.Create(
            "GeneratorTests",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }
}
