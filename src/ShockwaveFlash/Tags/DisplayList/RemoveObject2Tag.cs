namespace ShockwaveFlash.Tags.DisplayList;

public sealed class RemoveObject2Tag : Tag
{
    public ushort Depth { get; set; }

    public RemoveObject2Tag(TagMetadata metadata, ushort depth) : base(metadata)
    {
        Depth = depth;
    }

    public static RemoveObject2Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new RemoveObject2Tag(metadata, reader.ReadUInt16());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Depth);
    }
}
