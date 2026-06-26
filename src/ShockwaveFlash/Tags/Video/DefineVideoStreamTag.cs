using ShockwaveFlash.Types.Video;

namespace ShockwaveFlash.Tags.Video;

public sealed class DefineVideoStreamTag : Tag
{
    public ushort Id { get; set; }

    public ushort NumFrames { get; set; }

    public ushort Width { get; set; }

    public ushort Height { get; set; }

    public bool IsSmoothed { get; set; }

    public VideoDeblocking Deblocking { get; set; }

    public VideoCodec Codec { get; set; }

    public DefineVideoStreamTag(TagMetadata metadata, ushort id, ushort numFrames, ushort width, ushort height, bool isSmoothed, VideoDeblocking deblocking, VideoCodec codec) : base(metadata)
    {
        Id = id;
        NumFrames = numFrames;
        Width = width;
        Height = height;
        IsSmoothed = isSmoothed;
        Deblocking = deblocking;
        Codec = codec;
    }

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
