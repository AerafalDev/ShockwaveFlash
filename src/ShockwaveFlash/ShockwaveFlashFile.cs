// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers.Binary;
using ShockwaveFlash.Tags;

namespace ShockwaveFlash;

public sealed record ShockwaveFlashFile(ShockwaveFlashHeader Header, IReadOnlyList<Tag> Tags)
{
    public static ShockwaveFlashFile Disassemble(in ReadOnlySpan<byte> buffer)
    {
        var compression = (ShockwaveFlashCompression)buffer[0];
        var version = buffer[3];
        var fileLength = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(4, 4));

        var decompressed = compression.Decompress(buffer[8..], fileLength);

        if (decompressed.Length != fileLength)
            throw new InvalidDataException("Decompressed length does not match header length.");

        var reader = new SpanReader(decompressed);

        var header = ShockwaveFlashHeader.Decode(ref reader, compression, version, fileLength);

        var tags = Tag.DecodeCollection(ref reader, version);

        return new ShockwaveFlashFile(header, tags);
    }
}
