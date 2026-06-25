
namespace ShockwaveFlash.Tags.Video;

public sealed record VideoFrameTag(TagMetadata Metadata, ushort Id, ushort FrameNum, ReadOnlyMemory<byte> Data) : Tag(Metadata)
{
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
