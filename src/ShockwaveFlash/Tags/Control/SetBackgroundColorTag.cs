using ShockwaveFlash.Types;

namespace ShockwaveFlash.Tags.Control;

public sealed class SetBackgroundColorTag : Tag
{
    public Color BackgroundColor { get; set; }

    public SetBackgroundColorTag(TagMetadata metadata, Color backgroundColor) : base(metadata)
    {
        BackgroundColor = backgroundColor;
    }

    public static SetBackgroundColorTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new SetBackgroundColorTag(metadata, Color.DecodeRgb(reader));
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        BackgroundColor.EncodeRgb(writer);
    }
}
