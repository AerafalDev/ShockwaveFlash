// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Button;
using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags.Button;

public sealed record DefineButtonSoundTag(
    TagMetadata Metadata,
    ushort Id,
    ButtonSound? OverToUpSound,
    ButtonSound? UpToOverSound,
    ButtonSound? OverToDownSound,
    ButtonSound? DownToOverSound) : Tag(Metadata)
{
    public static DefineButtonSoundTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();

        var overToUpSound = reader.ReadUInt16() is var overToUpSoundId and not 0
            ? new ButtonSound(overToUpSoundId, SoundInfo.Decode(reader))
            : null;

        var upToOverSound = reader.ReadUInt16() is var upToOverSoundId and not 0
            ? new ButtonSound(upToOverSoundId, SoundInfo.Decode(reader))
            : null;

        var overToDownSound = reader.ReadUInt16() is var overToDownSoundId and not 0
            ? new ButtonSound(overToDownSoundId, SoundInfo.Decode(reader))
            : null;

        var downToOverSound = reader.ReadUInt16() is var downToOverSoundId and not 0
            ? new ButtonSound(downToOverSoundId, SoundInfo.Decode(reader))
            : null;

        return new DefineButtonSoundTag(metadata, id, overToUpSound, upToOverSound, overToDownSound, downToOverSound);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);

        EncodeSound(writer, OverToUpSound);
        EncodeSound(writer, UpToOverSound);
        EncodeSound(writer, OverToDownSound);
        EncodeSound(writer, DownToOverSound);
    }

    private static void EncodeSound(MemoryWriter writer, ButtonSound? sound)
    {
        if (sound is null)
        {
            writer.WriteUInt16(0);

            return;
        }

        writer.WriteUInt16(sound.Id);
        sound.SoundInfo.Encode(writer);
    }
}
