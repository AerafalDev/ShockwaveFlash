using ShockwaveFlash.Types;

namespace ShockwaveFlash.Tags.Control;

public sealed record SetBackgroundColorTag(TagMetadata Metadata, Color BackgroundColor) : Tag(Metadata)
{
    public static SetBackgroundColorTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new SetBackgroundColorTag(metadata, Color.DecodeRgb(reader));
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        BackgroundColor.EncodeRgb(writer);
    }
}
