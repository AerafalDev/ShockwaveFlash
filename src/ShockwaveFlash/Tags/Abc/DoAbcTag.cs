// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.


namespace ShockwaveFlash.Tags.Abc;

public sealed record DoAbcTag(TagMetadata Metadata, ReadOnlyMemory<byte> Data) : Tag(Metadata)
{
    public static DoAbcTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new DoAbcTag(metadata, reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteMemory(Data);
    }
}
