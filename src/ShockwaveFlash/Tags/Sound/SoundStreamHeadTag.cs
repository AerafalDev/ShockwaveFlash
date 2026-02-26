// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags.Sound;

public sealed record SoundStreamHeadTag(TagMetadata Metadata, SoundFormat StreamFormat, SoundFormat PlaybackFormat, ushort NumSamplesPerBlock, short LatencySeek) : Tag(Metadata)
{
    public static SoundStreamHeadTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var streamFormat = SoundFormat.Decode(ref reader);
        var playbackFormat = SoundFormat.Decode(ref reader);
        var numSamplesPerBlock = reader.ReadUInt16();
        var latencySeek = streamFormat.Compression is AudioCompression.Mp3 ? reader.ReadInt16() : (short)0;

        return new SoundStreamHeadTag(metadata, streamFormat, playbackFormat, numSamplesPerBlock, latencySeek);
    }
}
