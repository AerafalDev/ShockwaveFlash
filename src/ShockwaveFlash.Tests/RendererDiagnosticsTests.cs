using ShockwaveFlash;
using ShockwaveFlash.Rendering;
using ShockwaveFlash.Rendering.Diagnostics;
using ShockwaveFlash.Rendering.Exceptions;
using ShockwaveFlash.Tags;
using ShockwaveFlash.Tags.Control;
using ShockwaveFlash.Types;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class RendererDiagnosticsTests
{
    private sealed class CollectingSink : IDiagnosticSink
    {
        public List<RenderDiagnostic> Diagnostics { get; } = [];

        public void Report(RenderDiagnostic diagnostic)
        {
            Diagnostics.Add(diagnostic);
        }
    }

    private static ShockwaveFlashFile EmptyMovie()
    {
        var header = new ShockwaveFlashHeader(ShockwaveFlashCompression.None, 6, 0, new Rectangle(0, 0, 0, 0), new Fixed8(0), 1);
        return new ShockwaveFlashFile(header, [new EndTag(new TagMetadata(TagCode.End, 0, 0))]);
    }

    [Fact]
    public void Undefined_character_reports_a_diagnostic_and_degrades()
    {
        var sink = new CollectingSink();
        var renderer = new SwfRenderer(EmptyMovie(), new RenderOptions { Diagnostics = sink });

        renderer.Character(999).ShouldBe(MissingCharacter.Instance);
        sink.Diagnostics.ShouldContain(d => d.Severity == RenderSeverity.Warning && d.Message.Contains("999"));
    }

    [Fact]
    public void Undefined_character_throws_in_strict_mode()
    {
        var renderer = new SwfRenderer(EmptyMovie(), new RenderOptions { Strict = true });

        Should.Throw<RenderingException>(() => renderer.Character(999));
    }
}
