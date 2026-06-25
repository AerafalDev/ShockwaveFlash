namespace ShockwaveFlash.Tags.Control;

public sealed record SetTabIndexTag(TagMetadata Metadata, ushort Depth, ushort TabIndex) : Tag(Metadata)
{
    public static SetTabIndexTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new SetTabIndexTag(metadata, reader.ReadUInt16(), reader.ReadUInt16());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Depth);
        writer.WriteUInt16(TabIndex);
    }
}
