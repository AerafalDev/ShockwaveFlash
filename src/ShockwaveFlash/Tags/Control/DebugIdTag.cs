namespace ShockwaveFlash.Tags.Control;

public sealed class DebugIdTag : Tag
{
    public Guid Id { get; set; }

    public DebugIdTag(TagMetadata metadata, Guid id) : base(metadata)
    {
        Id = id;
    }

    public static DebugIdTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new DebugIdTag(metadata, new Guid(reader.ReadMemory(16).Span));
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        Span<byte> bytes = stackalloc byte[16];
        Id.TryWriteBytes(bytes);
        writer.WriteBytes(bytes);
    }
}
