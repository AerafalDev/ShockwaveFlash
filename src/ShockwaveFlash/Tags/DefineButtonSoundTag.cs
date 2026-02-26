// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Button;
using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags;

public sealed record DefineButtonSoundTag(
    TagMetadata Metadata,
    ushort Id,
    ButtonSound? OverToUpSound,
    ButtonSound? UpToOverSound,
    ButtonSound? OverToDownSound,
    ButtonSound? DownToOverSound) : Tag(Metadata)
{
    public static DefineButtonSoundTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var characterId = reader.ReadUInt16();

        var overToUpSound = reader.ReadUInt16() is var overToUpSoundId and not 0
            ? new ButtonSound(overToUpSoundId, SoundInfo.Decode(ref reader))
            : null;

        var upToOverSound = reader.ReadUInt16() is var upToOverSoundId and not 0
            ? new ButtonSound(upToOverSoundId, SoundInfo.Decode(ref reader))
            : null;

        var overToDownSound = reader.ReadUInt16() is var overToDownSoundId and not 0
            ? new ButtonSound(overToDownSoundId, SoundInfo.Decode(ref reader))
            : null;

        var downToOverSound = reader.ReadUInt16() is var downToOverSoundId and not 0
            ? new ButtonSound(downToOverSoundId, SoundInfo.Decode(ref reader))
            : null;

        return new DefineButtonSoundTag(metadata, characterId, overToUpSound, upToOverSound, overToDownSound, downToOverSound);
    }
}
