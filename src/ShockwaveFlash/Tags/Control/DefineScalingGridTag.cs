using ShockwaveFlash.Types;

namespace ShockwaveFlash.Tags.Control;

public sealed record DefineScalingGridTag(TagMetadata Metadata, ushort Id, Rectangle Splitter) : Tag(Metadata)
{
    public static DefineScalingGridTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new DefineScalingGridTag(metadata, reader.ReadUInt16(), Rectangle.Decode(reader));
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        Splitter.Encode(writer);
    }
}
