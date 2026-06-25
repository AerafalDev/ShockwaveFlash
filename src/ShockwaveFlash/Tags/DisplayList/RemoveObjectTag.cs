namespace ShockwaveFlash.Tags.DisplayList;

public sealed record RemoveObjectTag(TagMetadata Metadata, ushort Id, ushort Depth) : Tag(Metadata)
{
    public static RemoveObjectTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new RemoveObjectTag(metadata, reader.ReadUInt16(), reader.ReadUInt16());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteUInt16(Depth);
    }
}
