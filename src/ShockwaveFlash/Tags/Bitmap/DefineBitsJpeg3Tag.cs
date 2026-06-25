// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.


namespace ShockwaveFlash.Tags.Bitmap;

public sealed record DefineBitsJpeg3Tag(TagMetadata Metadata, ushort Id, ReadOnlyMemory<byte> Data, ReadOnlyMemory<byte> AlphaData) : Tag(Metadata)
{
    public static DefineBitsJpeg3Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var dataLength = reader.ReadInt32();
        var data = reader.ReadMemory(dataLength);
        var alphaData = reader.ReadMemoryToEnd();

        return new DefineBitsJpeg3Tag(metadata, id, data, alphaData);
    }
}
