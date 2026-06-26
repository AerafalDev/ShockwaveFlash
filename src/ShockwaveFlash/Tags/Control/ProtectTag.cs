namespace ShockwaveFlash.Tags.Control;

public sealed class ProtectTag : Tag
{
    public string? PasswordHash { get; set; }

    public ProtectTag(TagMetadata metadata, string? passwordHash) : base(metadata)
    {
        PasswordHash = passwordHash;
    }

    public static ProtectTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new ProtectTag(metadata, metadata.Length > 0 ? reader.ReadNullTerminatedString() : null);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        if (PasswordHash is not null)
            writer.WriteNullTerminatedString(PasswordHash);
    }
}
