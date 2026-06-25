namespace ShockwaveFlash.Tags.Action;

public sealed record DoInitActionTag(TagMetadata Metadata, ushort Id, ReadOnlyMemory<byte> Data) : Tag(Metadata)
{
    public static DoInitActionTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new DoInitActionTag(metadata, reader.ReadUInt16(), reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteMemory(Data);
    }
}
