namespace ShockwaveFlash.Tags.DisplayList;

public sealed record ShowFrameTag(TagMetadata Metadata) : Tag(Metadata)
{
    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
    }
}
