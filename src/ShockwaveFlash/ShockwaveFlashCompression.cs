// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

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
        public ReadOnlySpan<byte> Decompress(in ReadOnlySpan<byte> compressed, int uncompressedLength)
        {
            return self switch
            {
                ShockwaveFlashCompression.None => compressed,
                ShockwaveFlashCompression.ZLib => ZLib.Decompress(in compressed, uncompressedLength),
                ShockwaveFlashCompression.Lzma => Lzma.Decompress(in compressed, uncompressedLength),
                _ => throw new NotSupportedException($"Unsupported compression format: {self}.")
            };
        }
    }
}
