// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.


namespace ShockwaveFlash.Tags.Metadata;

public sealed record DefineBinaryDataTag(TagMetadata Metadata, ushort Id, ReadOnlyMemory<byte> Data) : Tag(Metadata)
{
    public static DefineBinaryDataTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        reader.Advance(sizeof(uint));
        var data = reader.ReadMemoryToEnd();

        return new DefineBinaryDataTag(metadata, id, data);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteUInt32(0);
        writer.WriteMemory(Data);
    }
}
