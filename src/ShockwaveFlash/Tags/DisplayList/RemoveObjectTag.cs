namespace ShockwaveFlash.Tags.DisplayList;

public sealed class RemoveObjectTag : Tag
{
    public ushort Id { get; set; }

    public ushort Depth { get; set; }

    public RemoveObjectTag(TagMetadata metadata, ushort id, ushort depth) : base(metadata)
    {
        Id = id;
        Depth = depth;
    }

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
