namespace ShockwaveFlash.Tags.Control;

public sealed record DebugIdTag(TagMetadata Metadata, Guid Id) : Tag(Metadata)
{
    public static DebugIdTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new DebugIdTag(metadata, new Guid(reader.ReadMemory(16).Span));
    }
}
