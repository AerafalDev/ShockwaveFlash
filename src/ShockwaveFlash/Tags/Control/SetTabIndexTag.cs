namespace ShockwaveFlash.Tags.Control;

public sealed class SetTabIndexTag : Tag
{
    public ushort Depth { get; set; }

    public ushort TabIndex { get; set; }

    public SetTabIndexTag(TagMetadata metadata, ushort depth, ushort tabIndex) : base(metadata)
    {
        Depth = depth;
        TabIndex = tabIndex;
    }

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
