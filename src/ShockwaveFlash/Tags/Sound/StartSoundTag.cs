// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags.Sound;

public sealed record StartSoundTag(TagMetadata Metadata, ushort Id, SoundInfo SoundInfo) : Tag(Metadata)
{
    public static StartSoundTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var soundInfo = SoundInfo.Decode(ref reader);

        return new StartSoundTag(metadata, id, soundInfo);
    }
}
