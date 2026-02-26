// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Video;

namespace ShockwaveFlash.Tags;

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
    public static DefineVideoStreamTag Decode(ref SpanReader reader, TagMetadata metadata)
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
}
