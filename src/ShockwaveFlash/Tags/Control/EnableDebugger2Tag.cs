namespace ShockwaveFlash.Tags.Control;

public sealed class EnableDebugger2Tag : Tag
{
    public string Password { get; set; }

    public EnableDebugger2Tag(TagMetadata metadata, string password) : base(metadata)
    {
        Password = password;
    }

    public static EnableDebugger2Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        reader.Advance(sizeof(ushort));

        return new EnableDebugger2Tag(metadata, reader.ReadNullTerminatedString());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(0);
        writer.WriteNullTerminatedString(Password);
    }
}
