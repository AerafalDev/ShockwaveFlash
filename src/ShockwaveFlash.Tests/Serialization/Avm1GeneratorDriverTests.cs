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
    private const string ContextSource = """
        using ShockwaveFlash.Avm1.Serialization;
        using ShockwaveFlash.Avm1.Serialization.Metadata;

        namespace Demo;

        public partial record Item([property: Avm1Property("n")] string Name, int Count);

        [Avm1Serializable(typeof(Item), "it")]
        public partial class DemoContext : Avm1SerializerContext;
        """;

    [Fact]
    public void Generates_a_serializer_context_for_a_registered_type()
    {
        var (result, diagnostics) = Run(ContextSource);

        diagnostics.ShouldBeEmpty();

        var generated = result.GeneratedSources.Single(s => s.HintName.EndsWith(".Avm1Context.g.cs", StringComparison.Ordinal));
        var text = generated.SourceText.ToString();

        text.ShouldContain("public static DemoContext Default");
        text.ShouldContain("public override");
        text.ShouldContain("GetTypeInfo(global::System.Type type)");
        text.ShouldContain("Avm1MetadataServices.CreateObjectInfo");
        text.ShouldContain("BindingPath = new string[] { \"it\" }");
    }

    [Fact]
    public void Reports_AVM1003_for_a_type_without_a_usable_constructor()
    {
        var (_, diagnostics) = Run("""
            using ShockwaveFlash.Avm1.Serialization;
            using ShockwaveFlash.Avm1.Serialization.Metadata;

            public partial class Ambiguous
            {
                public Ambiguous(int a) { }
                public Ambiguous(string b) { }
            }

            [Avm1Serializable(typeof(Ambiguous))]
            public partial class C : Avm1SerializerContext;
            """);

        diagnostics.Select(d => d.Id).ShouldContain("AVM1003");
    }

    [Fact]
    public void Reports_AVM1004_for_a_duplicate_member_key()
    {
        var (_, diagnostics) = Run("""
            using ShockwaveFlash.Avm1.Serialization;
            using ShockwaveFlash.Avm1.Serialization.Metadata;

            public partial record Dup([property: Avm1Property("k")] int A, [property: Avm1Property("k")] int B);

            [Avm1Serializable(typeof(Dup))]
            public partial class C : Avm1SerializerContext;
            """);

        diagnostics.Select(d => d.Id).ShouldContain("AVM1004");
    }

    [Fact]
    public void Reports_AVM1007_for_a_non_partial_context()
    {
        var (_, diagnostics) = Run("""
            using ShockwaveFlash.Avm1.Serialization;
            using ShockwaveFlash.Avm1.Serialization.Metadata;

            public partial record Item(string Name);

            [Avm1Serializable(typeof(Item))]
            public class NotPartialContext : Avm1SerializerContext;
            """);

        diagnostics.Select(d => d.Id).ShouldContain("AVM1007");
    }

    [Fact]
    public void Context_pipeline_output_is_cached_on_an_unrelated_edit()
    {
        var compilation = Compile(ContextSource);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new Avm1SerializableGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation);

        var edited = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("namespace Other { class Extra { } }"));
        driver = driver.RunGenerators(edited);

        var tracked = driver.GetRunResult().Results[0].TrackedSteps;
        tracked.ShouldContainKey("Avm1ParseContext");
        tracked["Avm1ParseContext"]
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
            MetadataReference.CreateFromFile(typeof(Avm1SerializableAttribute).Assembly.Location),
        };

        return CSharpCompilation.Create(
            "GeneratorTests",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }
}
