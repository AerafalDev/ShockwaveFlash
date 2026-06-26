namespace ShockwaveFlash.Tags.Control;

public sealed class NameCharacterTag : Tag
{
    public ushort Id { get; set; }

    public string Name { get; set; }

    public NameCharacterTag(TagMetadata metadata, ushort id, string name) : base(metadata)
    {
        Id = id;
        Name = name;
    }

    public static NameCharacterTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var name = reader.ReadNullTerminatedString();

        return new NameCharacterTag(metadata, id, name);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteNullTerminatedString(Name);
    }
}
