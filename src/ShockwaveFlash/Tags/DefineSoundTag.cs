// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;
using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags;

public sealed record DefineSoundTag(TagMetadata Metadata, ushort Id, SoundFormat Format, uint NumSamples, SpanSlice Data) : Tag(Metadata)
{
    public static DefineSoundTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var format = SoundFormat.Decode(ref reader);
        var numSamples = reader.ReadUInt32();
        var data = reader.SliceToEnd();

        return new DefineSoundTag(metadata, id, format, numSamples, data);
    }
}
