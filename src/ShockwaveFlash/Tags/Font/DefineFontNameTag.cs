namespace ShockwaveFlash.Tags.Font;

public sealed class DefineFontNameTag : Tag
{
    public ushort Id { get; set; }

    public string FontName { get; set; }

    public string Copyright { get; set; }

    public DefineFontNameTag(TagMetadata metadata, ushort id, string fontName, string copyright) : base(metadata)
    {
        Id = id;
        FontName = fontName;
        Copyright = copyright;
    }

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
