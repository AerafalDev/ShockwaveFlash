using ShockwaveFlash.Exceptions;
using ShockwaveFlash.IO.Compression;
using ShockwaveFlash.Rendering.Exceptions;
using ShockwaveFlash.Rendering.Model.Images;
using ShockwaveFlash.Tags.Bitmap;
using ShockwaveFlash.Types.Bitmap;
using SkiaSharp;

namespace ShockwaveFlash.Rendering.Processing;

public static class BitmapDecoder
{
    public static IImage Decode(DefineBitsLosslessTag tag)
    {
        return DecodeLossless(tag.Width, tag.Height, tag.Format, tag.ZLibBitmapData, false);
    }

    public static IImage Decode(DefineBitsLossless2Tag tag)
    {
        return DecodeLossless(tag.Width, tag.Height, tag.Format, tag.ZLibBitmapData, true);
    }

    public static IImage Decode(DefineBitsJpeg2Tag tag)
    {
        return DecodeJpeg(tag.Data, default);
    }

    public static IImage Decode(DefineBitsJpeg3Tag tag)
    {
        return DecodeJpeg(tag.Data, tag.AlphaData);
    }

    public static IImage Decode(DefineBitsJpeg4Tag tag)
    {
        return DecodeJpeg(tag.Data, tag.AlphaData);
    }

    private static RasterImage DecodeLossless(int width, int height, BitmapFormat format, ReadOnlyMemory<byte> zlib, bool hasAlpha)
    {
        int tableSize;
        int rowStride;

        switch (format)
        {
            case BitmapFormat.BitmapFormatColorMap8 colorMap:
                tableSize = colorMap.NumColors * (hasAlpha ? 4 : 3);
                rowStride = (width + 3) & ~3;
                break;

            case BitmapFormat.BitmapFormatRgb32:
                tableSize = 0;
                rowStride = width * 4;
                break;

            default:
                throw new RenderingException($"Bitmap format {format.GetType().Name} is not supported yet.");
        }

        var decompressed = ZLib.Decompress(zlib, tableSize + rowStride * height);
        var raw = decompressed.Span;
        var pixels = new SKColor[width * height];

        if (format is BitmapFormat.BitmapFormatColorMap8 colorMap8)
        {
            var entrySize = hasAlpha ? 4 : 3;
            var table = new SKColor[colorMap8.NumColors];

            for (var i = 0; i < table.Length; i++)
            {
                var offset = i * entrySize;
                table[i] = new SKColor(raw[offset], raw[offset + 1], raw[offset + 2], hasAlpha ? raw[offset + 3] : (byte)255);
            }

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var index = raw[tableSize + (y * rowStride) + x];
                    pixels[(y * width) + x] = index < table.Length ? table[index] : SKColors.Transparent;
                }
            }
        }
        else
        {
            for (var i = 0; i < pixels.Length; i++)
            {
                var offset = i * 4;

                if (!hasAlpha)
                {
                    pixels[i] = new SKColor(raw[offset + 1], raw[offset + 2], raw[offset + 3], 255);
                    continue;
                }

                var alpha = raw[offset];
                pixels[i] = alpha is 0
                    ? SKColors.Transparent
                    : new SKColor(
                        (byte)Math.Min(255, raw[offset + 1] * 255 / alpha),
                        (byte)Math.Min(255, raw[offset + 2] * 255 / alpha),
                        (byte)Math.Min(255, raw[offset + 3] * 255 / alpha),
                        alpha);
            }
        }

        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul) { Pixels = pixels };
        return Encode(bitmap, width, height);
    }

    private static RasterImage DecodeJpeg(ReadOnlyMemory<byte> data, ReadOnlyMemory<byte> alpha)
    {
        using var bitmap = SKBitmap.Decode(data.ToArray())
            ?? throw new RenderingException("Failed to decode the embedded JPEG image.");

        if (!alpha.IsEmpty)
            ApplyAlpha(bitmap, alpha);

        return Encode(bitmap, bitmap.Width, bitmap.Height);
    }

    private static void ApplyAlpha(SKBitmap bitmap, ReadOnlyMemory<byte> alpha)
    {
        var count = bitmap.Width * bitmap.Height;
        byte[] plane;

        if (alpha.Length == count)
        {
            plane = alpha.ToArray();
        }
        else
        {
            try
            {
                plane = ZLib.Decompress(alpha, count).ToArray();
            }
            catch (SwfException)
            {
                return;
            }
        }

        if (plane.Length < count)
            return;

        var pixels = bitmap.Pixels;

        for (var i = 0; i < count; i++)
            pixels[i] = pixels[i].WithAlpha(plane[i]);

        bitmap.Pixels = pixels;
    }

    private static RasterImage Encode(SKBitmap bitmap, int width, int height)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return new RasterImage(data.ToArray(), width, height);
    }
}
