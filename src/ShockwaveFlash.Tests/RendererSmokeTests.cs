using ShockwaveFlash;
using ShockwaveFlash.Rendering;
using ShockwaveFlash.Rendering.Drawing.Svg;
using ShockwaveFlash.Tags.Shape;
using ShockwaveFlash.Tags.Sprite;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class RendererSmokeTests
{
    [Fact]
    public void Every_shape_and_sprite_in_a_sample_renders_to_svg_without_throwing()
    {
        var failures = new List<string>();

        foreach (var path in Corpus.Files().OrderBy(static path => new FileInfo(path).Length).Take(6))
        {
            var file = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));
            var renderer = new SwfRenderer(file);

            var ids = file.Tags
                .Select(static tag => tag switch
                {
                    DefineShapeTag shape => (int?)shape.ShapeId,
                    DefineSpriteTag sprite => sprite.Id,
                    _ => null
                })
                .OfType<int>()
                .Take(80);

            foreach (var id in ids)
            {
                try
                {
                    var svg = SvgDrawer.RenderToSvg(renderer.Character(id));
                    svg.ShouldStartWith("<svg");
                    svg.ShouldContain("</svg>");
                }
                catch (Exception ex)
                {
                    failures.Add($"{Path.GetFileName(path)} character {id}: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        failures.ShouldBeEmpty();
    }
}
