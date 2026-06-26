using ShockwaveFlash;
using ShockwaveFlash.Rendering;
using ShockwaveFlash.Rendering.Drawing.Svg;
using ShockwaveFlash.Tags.Control;
using ShockwaveFlash.Tags.Shape;
using ShockwaveFlash.Tags.Sprite;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class RendererSmokeTests
{
    [Fact]
    public void The_root_movie_renders_at_the_stage_size_with_its_background()
    {
        var failures = new List<string>();

        foreach (var path in Corpus.Files().OrderBy(static path => new FileInfo(path).Length).Take(8))
        {
            var file = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));
            var movie = new SwfRenderer(file).Movie();

            movie.Bounds.ShouldBe(file.Header.FrameSize);

            var declared = file.Tags.OfType<SetBackgroundColorTag>().LastOrDefault()?.BackgroundColor;
            movie.BackgroundColor.ShouldBe(declared);

            try
            {
                var svg = SvgDrawer.RenderToSvg(movie);
                svg.ShouldStartWith("<svg");

                if (declared is { } color && color.A > 0)
                    svg.ShouldContain(color.ToHexRgb());
            }
            catch (Exception ex)
            {
                failures.Add($"{Path.GetFileName(path)}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        failures.ShouldBeEmpty();
    }

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
