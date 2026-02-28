// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.IO.Compression;

public static class Lzma
{
    public static ReadOnlySpan<byte> Decompress(ReadOnlySpan<byte> compressed, int uncompressedLength)
    {
        throw new NotSupportedException(
            "ZWS (LZMA-compressed) SWF files require an LZMA decoder. " +
            "SPEC GAP: .NET 10 BCL does not include a built-in LZMA implementation. " +
            "Add a NuGet reference to a library providing LZMA support (e.g. SharpCompress) " +
            "and replace this method body with the appropriate decompression call.");
    }
}
