namespace ShockwaveFlash.Tags.Control;

public sealed record ProtectTag(TagMetadata Metadata, string? PasswordHash) : Tag(Metadata)
{
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
