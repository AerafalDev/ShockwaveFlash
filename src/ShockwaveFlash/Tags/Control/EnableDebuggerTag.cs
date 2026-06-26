namespace ShockwaveFlash.Tags.Control;

public sealed class EnableDebuggerTag : Tag
{
    public string Password { get; set; }

    public EnableDebuggerTag(TagMetadata metadata, string password) : base(metadata)
    {
        Password = password;
    }

    public static EnableDebuggerTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new EnableDebuggerTag(metadata, reader.ReadNullTerminatedString());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteNullTerminatedString(Password);
    }
}
