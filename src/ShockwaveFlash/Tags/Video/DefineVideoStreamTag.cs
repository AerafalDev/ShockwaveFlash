using ShockwaveFlash.Types.Video;

namespace ShockwaveFlash.Tags.Video;

public sealed record DefineVideoStreamTag(
    TagMetadata Metadata,
    ushort Id,
    ushort NumFrames,
    ushort Width,
    ushort Height,
    bool IsSmoothed,
    VideoDeblocking Deblocking,
    VideoCodec Codec) : Tag(Metadata)
{
    public static DefineVideoStreamTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var numFrames = reader.ReadUInt16();
        var width = reader.ReadUInt16();
        var height = reader.ReadUInt16();
        var flags = reader.ReadUInt8();
        var isSmoothed = (flags & 1) is not 0;
        var deblocking = (VideoDeblocking)((flags >> 1) & 7);
        var codec = (VideoCodec)reader.ReadUInt8();

        return new DefineVideoStreamTag(metadata, id, numFrames, width, height, isSmoothed, deblocking, codec);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteUInt16(NumFrames);
        writer.WriteUInt16(Width);
        writer.WriteUInt16(Height);

        var flags = (byte)((((byte)Deblocking & 7) << 1) | (IsSmoothed ? 1 : 0));

        writer.WriteUInt8(flags);
        writer.WriteUInt8((byte)Codec);
    }
}
