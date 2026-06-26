namespace ShockwaveFlash.Tags.Control;

public sealed class FrameLabelTag : Tag
{
    public string Name { get; set; }

    public bool IsAnchor { get; set; }

    public FrameLabelTag(TagMetadata metadata, string name, bool isAnchor) : base(metadata)
    {
        Name = name;
        IsAnchor = isAnchor;
    }

    public static FrameLabelTag Decode(MemoryReader reader, TagMetadata metadata, byte swfVersion)
    {
        var name = reader.ReadNullTerminatedString();
        var isAnchor = false;

        if (swfVersion >= 6 && reader.Remaining > 0)
            isAnchor = reader.ReadBoolean();

        return new FrameLabelTag(metadata, name, isAnchor);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteNullTerminatedString(Name);

        if (IsAnchor)
            writer.WriteUInt8(1);
    }
}
