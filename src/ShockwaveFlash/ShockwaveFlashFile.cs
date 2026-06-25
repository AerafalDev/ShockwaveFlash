// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Tags;

namespace ShockwaveFlash;

public sealed record ShockwaveFlashFile(ShockwaveFlashHeader Header, IReadOnlyList<Tag> Tags)
{
    private const int HeaderSize = 8;

    public static ShockwaveFlashFile Disassemble(ReadOnlyMemory<byte> data)
    {
        var reader = new MemoryReader(data);

        var compression = (ShockwaveFlashCompression)reader.ReadUInt8();

        reader.Advance(sizeof(ushort));

        var version = reader.ReadUInt8();
        var fileLength = reader.ReadInt32();
        var bodyLength = fileLength - HeaderSize;

        reader = new MemoryReader(compression.Decompress(reader.ReadMemoryToEnd(), bodyLength));

        var header = ShockwaveFlashHeader.Decode(reader, compression, version, fileLength);

        var tags = Tag.DecodeCollection(reader, version);

        return new ShockwaveFlashFile(header, tags);
    }
}
