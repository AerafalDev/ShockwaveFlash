namespace ShockwaveFlash.Tags.Font;

public sealed record DefineFontNameTag(TagMetadata Metadata, ushort Id, string FontName, string Copyright) : Tag(Metadata)
{
    public static DefineFontNameTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var name = reader.ReadNullTerminatedString();
        var copyright = reader.ReadNullTerminatedString();

        return new DefineFontNameTag(metadata, id, name, copyright);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteNullTerminatedString(FontName);
        writer.WriteNullTerminatedString(Copyright);
    }
}
