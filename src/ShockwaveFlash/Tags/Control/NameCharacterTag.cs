namespace ShockwaveFlash.Tags.Control;

public sealed class NameCharacterTag : Tag
{
    public ushort Id { get; set; }

    public string Name { get; set; }

    public ushort? Type { get; set; }

    public NameCharacterTag(TagMetadata metadata, ushort id, string name, ushort? type) : base(metadata)
    {
        Id = id;
        Name = name;
        Type = type;
    }

    public static NameCharacterTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var name = reader.ReadNullTerminatedString();
        ushort? type = null;

        if (reader.Remaining > 0)
            type = reader.ReadUInt16();

        return new NameCharacterTag(metadata, id, name, type);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteNullTerminatedString(Name);

        if (Type.HasValue)
            writer.WriteUInt16(Type.Value);
    }
}
