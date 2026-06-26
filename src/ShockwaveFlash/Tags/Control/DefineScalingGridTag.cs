using ShockwaveFlash.Types;

namespace ShockwaveFlash.Tags.Control;

public sealed class DefineScalingGridTag : Tag
{
    public ushort Id { get; set; }

    public Rectangle Splitter { get; set; }

    public DefineScalingGridTag(TagMetadata metadata, ushort id, Rectangle splitter) : base(metadata)
    {
        Id = id;
        Splitter = splitter;
    }

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
