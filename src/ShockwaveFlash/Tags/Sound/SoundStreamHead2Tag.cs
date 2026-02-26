// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags.Sound;

public sealed record SoundStreamHead2Tag(TagMetadata Metadata, SoundFormat StreamFormat, SoundFormat PlaybackFormat, ushort NumSamplesPerBlock, short LatencySeek) : Tag(Metadata)
{
    public static SoundStreamHead2Tag Decode(ref SpanReader reader, TagMetadata metadata, byte tagVersion)
    {
        var streamFormat = SoundFormat.Decode(ref reader);
        var playbackFormat = SoundFormat.Decode(ref reader);
        var numSamplesPerBlock = reader.ReadUInt16();
        var latencySeek = streamFormat.Compression is AudioCompression.Mp3 ? reader.ReadInt16() : (short)0;

        return new SoundStreamHead2Tag(metadata, streamFormat, playbackFormat, numSamplesPerBlock, latencySeek);
    }
}
