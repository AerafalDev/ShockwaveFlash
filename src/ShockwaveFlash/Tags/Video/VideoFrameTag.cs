// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.


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
}
