using ShockwaveFlash.Exceptions;

namespace ShockwaveFlash.IO.Compression;

public static class Lzma
{
    public static ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> compressed, int uncompressedLength)
    {
        throw new SwfUnsupportedException(
            "ZWS (LZMA-compressed) SWF files require an LZMA decoder. " +
            "The .NET BCL does not include a built-in LZMA implementation. " +
            "Add an LZMA library (e.g. SharpCompress) and replace this method body.");
    }

    public static ReadOnlyMemory<byte> Compress(ReadOnlyMemory<byte> data)
    {
        throw new SwfUnsupportedException(
            "Writing ZWS (LZMA-compressed) SWF files requires an LZMA encoder. " +
            "The .NET BCL does not include a built-in LZMA implementation. " +
            "Add an LZMA library (e.g. SharpCompress) and replace this method body.");
    }
}
