using ShockwaveFlash;
using ShockwaveFlash.Rendering;
using ShockwaveFlash.Rendering.Drawing.Svg;
using ShockwaveFlash.Tags.Button;
using ShockwaveFlash.Tags.Control;
using ShockwaveFlash.Tags.Shape;
using ShockwaveFlash.Tags.Sprite;
using ShockwaveFlash.Types.Button;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class RendererSmokeTests
{
    [Fact]
    public void Every_button_renders_each_of_its_four_states_without_throwing()
    {
        var failures = new List<string>();
        ButtonStates[] states = [ButtonStates.Up, ButtonStates.Over, ButtonStates.Down, ButtonStates.HitTest];

        foreach (var path in Corpus.Files())
        {
            var file = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));

            if (!file.Tags.OfType<DefineButton2Tag>().Any())
                continue;

            var renderer = new SwfRenderer(file);

            foreach (var button2 in file.Tags.OfType<DefineButton2Tag>())
            {
                if (renderer.Button(button2.Id) is not { } button)
                    continue;

                foreach (var state in states)
                {
                    try
                    {
                        SvgDrawer.RenderToSvg(button.ForState(state)).ShouldStartWith("<svg");
                    }
                    catch (Exception ex)
                    {
                        failures.Add($"{Path.GetFileName(path)} button {button2.Id} {state}: {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }
        }

        failures.ShouldBeEmpty();
    }

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
