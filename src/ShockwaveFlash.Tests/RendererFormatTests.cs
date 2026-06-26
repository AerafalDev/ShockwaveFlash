using System.Text;
using ShockwaveFlash;
using ShockwaveFlash.Rendering;
using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Rendering.Drawing.Skia;
using ShockwaveFlash.Tags.Shape;
using ShockwaveFlash.Tags.Sprite;
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

    [Fact]
    public void Renders_an_animated_sprite_to_a_valid_animated_gif()
    {
        if (FirstAnimatedSprite() is not var (drawable, frames))
            return;

        var gif = SkiaDrawer.RenderToAnimatedGif(drawable, 80, 1f, White);

        Encoding.ASCII.GetString(gif, 0, 6).ShouldBe("GIF89a");
        gif[^1].ShouldBe((byte)0x3B);
        ImageDescriptorCount(gif).ShouldBe(frames);
    }

    private static (IDrawable Drawable, int Frames)? FirstAnimatedSprite()
    {
        foreach (var path in Corpus.Files())
        {
            var file = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));
            var sprite = file.Tags.OfType<DefineSpriteTag>().FirstOrDefault(static tag => tag.NumFrames is > 1 and < 40);

            if (sprite is not null)
            {
                var drawable = new SwfRenderer(file).Sprite(sprite.Id)!;
                return (drawable, drawable.FrameCount());
            }
        }

        return null;
    }

    private static int ImageDescriptorCount(byte[] gif)
    {
        var pos = 13;
        var packed = gif[10];

        if ((packed & 0x80) != 0)
            pos += 3 * (1 << ((packed & 7) + 1));

        var count = 0;

        while (pos < gif.Length)
        {
            var block = gif[pos++];

            if (block == 0x3B)
                break;

            if (block == 0x21)
            {
                pos++;
                pos = SkipSubBlocks(gif, pos);
            }
            else if (block == 0x2C)
            {
                var local = gif[pos + 8];
                pos += 9;

                if ((local & 0x80) != 0)
                    pos += 3 * (1 << ((local & 7) + 1));

                pos++;
                pos = SkipSubBlocks(gif, pos);
                count++;
            }
        }

        return count;
    }

    private static int SkipSubBlocks(byte[] gif, int pos)
    {
        while (true)
        {
            var length = gif[pos++];

            if (length == 0)
                return pos;

            pos += length;
        }
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
