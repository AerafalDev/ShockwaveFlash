// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Tags.Video;

public sealed record VideoFrameTag(TagMetadata Metadata, ushort Id, ushort FrameNum, SpanSlice Data) : Tag(Metadata)
{
    public static VideoFrameTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var frameNum = reader.ReadUInt16();
        var data = reader.SliceToEnd();

        return new VideoFrameTag(metadata, id, frameNum, data);
    }
}
