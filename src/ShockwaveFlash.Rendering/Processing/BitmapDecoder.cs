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

    public static IImage Decode(DefineBitsTag tag, JpegTablesTag? tables)
    {
        if (tables is null || tables.Data.IsEmpty)
            return DecodeJpeg(tag.ImageData, default);

        var combined = new byte[tables.Data.Length + tag.ImageData.Length];
        tables.Data.Span.CopyTo(combined);
        tag.ImageData.Span.CopyTo(combined.AsSpan(tables.Data.Length));

        return DecodeJpeg(combined, default);
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

            case BitmapFormat.BitmapFormatRgb15:
                tableSize = 0;
                rowStride = (width * 2 + 3) & ~3;
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
        else if (format is BitmapFormat.BitmapFormatRgb15)
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var offset = (y * rowStride) + (x * 2);
                    var value = (raw[offset] << 8) | raw[offset + 1];

                    pixels[(y * width) + x] = new SKColor(
                        Expand5((value >> 10) & 31),
                        Expand5((value >> 5) & 31),
                        Expand5(value & 31),
                        255);
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

    private static byte Expand5(int value)
    {
        return (byte)(((value & 31) * 255 + 15) / 31);
    }

    private static byte[] FixJpeg(ReadOnlyMemory<byte> data)
    {
        var bytes = data.Span;
        var result = new List<byte>(bytes.Length);

        for (var i = 0; i < bytes.Length;)
        {
            if (i + 3 < bytes.Length && bytes[i] is 0xFF && bytes[i + 1] is 0xD9 && bytes[i + 2] is 0xFF && bytes[i + 3] is 0xD8)
            {
                i += 4;
                continue;
            }

            result.Add(bytes[i]);
            i++;
        }

        return result.ToArray();
    }

    private static RasterImage DecodeJpeg(ReadOnlyMemory<byte> data, ReadOnlyMemory<byte> alpha)
    {
        using var decoded = SKBitmap.Decode(FixJpeg(data))
            ?? throw new RenderingException("Failed to decode the embedded JPEG image.");

        var plane = alpha.IsEmpty ? null : DecompressAlpha(alpha, decoded.Width * decoded.Height);

        if (plane is null)
            return Encode(decoded, decoded.Width, decoded.Height);

        var source = decoded.Pixels;
        var pixels = new SKColor[source.Length];

        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = source[i].WithAlpha(plane[i]);

        using var bitmap = new SKBitmap(decoded.Width, decoded.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul) { Pixels = pixels };
        return Encode(bitmap, decoded.Width, decoded.Height);
    }

    private static byte[]? DecompressAlpha(ReadOnlyMemory<byte> alpha, int count)
    {
        if (alpha.Length == count)
            return alpha.ToArray();

        try
        {
            var plane = ZLib.Decompress(alpha, count).ToArray();
            return plane.Length >= count ? plane : null;
        }
        catch (SwfException)
        {
            return null;
        }
    }

    private static RasterImage Encode(SKBitmap bitmap, int width, int height)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return new RasterImage(data.ToArray(), width, height);
    }
}
