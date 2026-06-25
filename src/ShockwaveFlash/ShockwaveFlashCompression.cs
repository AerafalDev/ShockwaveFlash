using ShockwaveFlash.Exceptions;
using ShockwaveFlash.IO.Compression;

namespace ShockwaveFlash;

public enum ShockwaveFlashCompression : byte
{
    None = (byte)'F',
    ZLib = (byte)'C',
    Lzma = (byte)'Z'
}

public static class ShockwaveFlashCompressionExtensions
{
    extension(ShockwaveFlashCompression self)
    {
        public ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> compressed, int uncompressedLength)
        {
            return self switch
            {
                ShockwaveFlashCompression.None => compressed,
                ShockwaveFlashCompression.ZLib => ZLib.Decompress(compressed, uncompressedLength),
                ShockwaveFlashCompression.Lzma => Lzma.Decompress(compressed, uncompressedLength),
                _ => throw new SwfUnsupportedException($"Unsupported compression format: {self}.")
            };
        }

        public ReadOnlyMemory<byte> Compress(ReadOnlyMemory<byte> data)
        {
            return self switch
            {
                ShockwaveFlashCompression.None => data,
                ShockwaveFlashCompression.ZLib => ZLib.Compress(data),
                ShockwaveFlashCompression.Lzma => Lzma.Compress(data),
                _ => throw new SwfUnsupportedException($"Unsupported compression format: {self}.")
            };
        }
    }
}
