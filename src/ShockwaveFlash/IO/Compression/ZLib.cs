// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO.Compression;

namespace ShockwaveFlash.IO.Compression;

public static class ZLib
{
    private const int MaxDecompressedSize = 512 * 1024 * 1024;

    public static unsafe ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> compressed, int uncompressedLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(uncompressedLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(uncompressedLength, MaxDecompressedSize);

        var decompressed = new byte[uncompressedLength];
        var decompressedSpan = decompressed.AsSpan();

        using var handle = compressed.Pin();
        using var ms = new UnmanagedMemoryStream((byte*)handle.Pointer, compressed.Length);
        using var zs = new ZLibStream(ms, CompressionMode.Decompress, false);

        var reads = 0;

        while (reads < uncompressedLength)
        {
            var bytesRead = zs.Read(decompressedSpan[reads..]);

            if (bytesRead is 0)
                break;

            reads += bytesRead;
        }

        if (reads != uncompressedLength)
            throw new InvalidOperationException($"Expected {uncompressedLength} bytes but decompressed {reads} bytes.");

        return decompressed.AsMemory(0, reads);
    }

    public static ReadOnlyMemory<byte> Compress(ReadOnlyMemory<byte> data)
    {
        using var ms = new MemoryStream();

        using (var zs = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true))
            zs.Write(data.Span);

        return ms.ToArray();
    }
}
