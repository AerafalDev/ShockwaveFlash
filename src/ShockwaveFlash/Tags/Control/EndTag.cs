namespace ShockwaveFlash.Tags.Control;

public sealed record EndTag(TagMetadata Metadata) : Tag(Metadata)
{
    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
    }
}
