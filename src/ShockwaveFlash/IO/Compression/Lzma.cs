using System.Buffers.Binary;
using SharpCompress.Compressors.LZMA;
using ShockwaveFlash.Exceptions;

namespace ShockwaveFlash.IO.Compression;

internal static class Lzma
{
    private const int HeaderSize = 9;

    private const int MaxDecompressedSize = 512 * 1024 * 1024;

    public static ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> compressed, int uncompressedLength)
    {
        if (uncompressedLength < 0)
            throw new SwfCompressionException($"Invalid uncompressed length {uncompressedLength} for an LZMA (ZWS) SWF.");

        if (uncompressedLength > MaxDecompressedSize)
            throw new SwfCompressionException($"Uncompressed length {uncompressedLength} exceeds the {MaxDecompressedSize}-byte limit.");

        if (compressed.Length < HeaderSize)
            throw new SwfCompressionException("The ZWS body is too small to contain an LZMA header.");

        var properties = compressed.Slice(4, 5).ToArray();
        var data = compressed[HeaderSize..].ToArray();

        try
        {
            using var input = new MemoryStream(data, writable: false);
            using var lzma = new LzmaStream(properties, input, data.Length, uncompressedLength);

            var output = new byte[uncompressedLength];
            var read = 0;

            while (read < uncompressedLength)
            {
                var count = lzma.Read(output, read, uncompressedLength - read);

                if (count <= 0)
                    break;

                read += count;
            }

            if (read != uncompressedLength)
                throw new SwfCompressionException($"The LZMA stream produced {read} of {uncompressedLength} expected bytes.");

            return output;
        }
        catch (Exception exception) when (exception is not SwfException)
        {
            throw new SwfCompressionException("Failed to decompress the LZMA (ZWS) SWF body.", exception);
        }
    }

    public static ReadOnlyMemory<byte> Compress(ReadOnlyMemory<byte> data)
    {
        try
        {
            using var output = new MemoryStream();
            byte[] properties;

            using (var lzma = new LzmaStream(new LzmaEncoderProperties(), false, output))
            {
                properties = lzma.Properties;

                var payload = data.ToArray();
                lzma.Write(payload, 0, payload.Length);
            }

            var compressed = output.ToArray();
            var result = new byte[HeaderSize + compressed.Length];

            BinaryPrimitives.WriteUInt32LittleEndian(result, (uint)compressed.Length);
            properties.CopyTo(result, 4);
            compressed.CopyTo(result, HeaderSize);

            return result;
        }
        catch (Exception exception) when (exception is not SwfException)
        {
            throw new SwfCompressionException("Failed to compress the SWF body with LZMA.", exception);
        }
    }
}
