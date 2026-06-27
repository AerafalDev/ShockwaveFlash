using System.IO.Compression;
using ShockwaveFlash.Exceptions;

namespace ShockwaveFlash.IO.Compression;

internal static class ZLib
{
    private const int MaxDecompressedSize = 512 * 1024 * 1024;

    public static unsafe ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> compressed, int uncompressedLength)
    {
        if (uncompressedLength <= 0)
            throw new SwfCompressionException($"Invalid uncompressed length {uncompressedLength}.");

        if (uncompressedLength > MaxDecompressedSize)
            throw new SwfCompressionException($"Uncompressed length {uncompressedLength} exceeds the {MaxDecompressedSize}-byte limit.");

        var decompressed = new byte[uncompressedLength];
        var decompressedSpan = decompressed.AsSpan();

        using var handle = compressed.Pin();
        using var ms = new UnmanagedMemoryStream((byte*)handle.Pointer, compressed.Length);
        using var zs = new ZLibStream(ms, CompressionMode.Decompress, false);

        var reads = 0;

        try
        {
            while (reads < uncompressedLength)
            {
                var bytesRead = zs.Read(decompressedSpan[reads..]);

                if (bytesRead is 0)
                    break;

                reads += bytesRead;
            }
        }
        catch (InvalidDataException e)
        {
            throw new SwfCompressionException("The zlib stream is corrupt.", e);
        }

        if (reads != uncompressedLength)
            throw new SwfCompressionException($"Expected {uncompressedLength} bytes but decompressed {reads} bytes.");

        return decompressed.AsMemory(0, reads);
    }

    public static ReadOnlyMemory<byte> Compress(ReadOnlyMemory<byte> data)
    {
        using var ms = new MemoryStream(data.Length < 2 ? 256 : data.Length / 2);

        using (var zs = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true))
            zs.Write(data.Span);

        return ms.GetBuffer().AsMemory(0, (int)ms.Length);
    }
}
