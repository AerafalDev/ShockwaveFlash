// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.


namespace ShockwaveFlash.Tags.Bitmap;

public sealed record DefineBitsJpeg4Tag(TagMetadata Metadata, ushort Id, float Deblocking, ReadOnlyMemory<byte> Data, ReadOnlyMemory<byte> AlphaData) : Tag(Metadata)
{
    public static DefineBitsJpeg4Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var dataLength = reader.ReadInt32();
        var deblocking = reader.ReadFixed8();
        var data = reader.ReadMemory(dataLength);
        var alphaData = reader.ReadMemoryToEnd();

        return new DefineBitsJpeg4Tag(metadata, id, deblocking, data, alphaData);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteInt32(Data.Length);
        writer.WriteFixed8(Deblocking);
        writer.WriteMemory(Data);
        writer.WriteMemory(AlphaData);
    }
}
