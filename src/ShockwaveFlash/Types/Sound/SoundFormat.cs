// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Sound;

public sealed record SoundFormat(AudioCompression Compression, ushort SampleRate, bool IsStereo, bool Is16Bit)
{
    public static SoundFormat Decode(ref SpanReader reader)
    {
        var flags = reader.ReadUInt8();
        var compression = (AudioCompression)(flags >> 4);
        ushort sampleRate = ((flags & 12) >> 2) switch
        {
            0 => 5512,
            1 => 11025,
            2 => 22050,
            3 => 44100,
            _ => throw new NotSupportedException("Not supported sample rate.")
        };
        var is16Bit = (flags & 2) is not 0;
        var isStereo = (flags & 1) is not 0;

        return new SoundFormat(compression, sampleRate, isStereo, is16Bit);
    }
}
