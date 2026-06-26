namespace ShockwaveFlash.Tags.Control;

public sealed class EndTag : Tag
{
    public EndTag(TagMetadata metadata) : base(metadata)
    {
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
    }
}
