
namespace ShockwaveFlash.Tags.Video;

public sealed class VideoFrameTag : Tag
{
    public ushort Id { get; set; }

    public ushort FrameNum { get; set; }

    public ReadOnlyMemory<byte> Data { get; set; }

    public VideoFrameTag(TagMetadata metadata, ushort id, ushort frameNum, ReadOnlyMemory<byte> data) : base(metadata)
    {
        Id = id;
        FrameNum = frameNum;
        Data = data;
    }

    public static VideoFrameTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var frameNum = reader.ReadUInt16();
        var data = reader.ReadMemoryToEnd();

        return new VideoFrameTag(metadata, id, frameNum, data);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteUInt16(FrameNum);
        writer.WriteMemory(Data);
    }
}
