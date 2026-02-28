// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Tags;

namespace ShockwaveFlash;

public sealed record ShockwaveFlashFile(ShockwaveFlashHeader Header, IReadOnlyList<Tag> Tags)
{
    public static ShockwaveFlashFile Disassemble(ref SpanReader reader)
    {
        var compression = (ShockwaveFlashCompression)reader.ReadUInt8();

        reader.Advance(sizeof(ushort));

        var version = reader.ReadUInt8();
        var fileLength = reader.ReadInt32();

        reader = new SpanReader(compression.Decompress(reader.ReadSpanToEnd(), fileLength));

        var header = ShockwaveFlashHeader.Decode(ref reader, compression, version, fileLength);

        var tags = Tag.DecodeCollection(ref reader, version);

        return new ShockwaveFlashFile(header, tags);
    }
}
