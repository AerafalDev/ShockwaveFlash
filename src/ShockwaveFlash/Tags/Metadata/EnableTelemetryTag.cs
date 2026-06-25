
namespace ShockwaveFlash.Tags.Metadata;

public sealed record EnableTelemetryTag(TagMetadata Metadata, ReadOnlyMemory<byte> PasswordHash) : Tag(Metadata)
{
    public static EnableTelemetryTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        reader.Advance(sizeof(ushort));

        return new EnableTelemetryTag(metadata, metadata.Length > 2 ? reader.ReadMemory(32) : ReadOnlyMemory<byte>.Empty);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(0);

        if (!PasswordHash.IsEmpty)
            writer.WriteMemory(PasswordHash);
    }
}
