using System.Text;
using ShockwaveFlash;
using ShockwaveFlash.Rendering;
using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Rendering.Drawing.Skia;
using ShockwaveFlash.Tags.Shape;
using Shouldly;
using SkiaSharp;

namespace ShockwaveFlash.Tests;

public sealed class RendererFormatTests
{
    private static readonly SKColor White = new(0xFF, 0xFF, 0xFF, 0xFF);

    [Fact]
    public void Renders_to_png_jpeg_and_webp()
    {
        if (FirstShape() is not { } drawable)
            return;

        SkiaDrawer.RenderToImage(drawable, SKEncodedImageFormat.Png, 100, 1f, White)
            .AsSpan(0, 4).ToArray().ShouldBe([0x89, 0x50, 0x4E, 0x47]);

        SkiaDrawer.RenderToImage(drawable, SKEncodedImageFormat.Jpeg, 90, 1f, White)
            .AsSpan(0, 3).ToArray().ShouldBe([0xFF, 0xD8, 0xFF]);

        var webp = SkiaDrawer.RenderToImage(drawable, SKEncodedImageFormat.Webp, 90, 1f, White);
        Encoding.ASCII.GetString(webp, 0, 4).ShouldBe("RIFF");
    }

    [Fact]
    public void Renders_to_pdf()
    {
        if (FirstShape() is not { } drawable)
            return;

        var pdf = SkiaDrawer.RenderToPdf(drawable, 1f, White);
        Encoding.ASCII.GetString(pdf, 0, 4).ShouldBe("%PDF");
    }

    private static IDrawable? FirstShape()
    {
        foreach (var path in Corpus.Files())
        {
            var file = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));
            var shape = file.Tags.OfType<DefineShapeTag>().FirstOrDefault(static tag => tag.ShapeBounds.Width > 40);

            if (shape is not null)
                return new SwfRenderer(file).Character(shape.ShapeId);
        }

        return null;
    }
}
