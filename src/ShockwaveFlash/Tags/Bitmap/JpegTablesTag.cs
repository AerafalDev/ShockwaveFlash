// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.


namespace ShockwaveFlash.Tags.Bitmap;

public sealed record JpegTablesTag(TagMetadata Metadata, ReadOnlyMemory<byte> Data) : Tag(Metadata)
{
    public static JpegTablesTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new JpegTablesTag(metadata, reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteMemory(Data);
    }
}
