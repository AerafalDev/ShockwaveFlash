// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.


namespace ShockwaveFlash.Tags.Bitmap;

public sealed record DefineBitsTag(TagMetadata Metadata, ushort Id, ReadOnlyMemory<byte> ImageData) : Tag(Metadata)
{
    public static DefineBitsTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new DefineBitsTag(metadata, reader.ReadUInt16(), reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteMemory(ImageData);
    }
}
