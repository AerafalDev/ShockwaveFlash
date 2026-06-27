using ShockwaveFlash.Exceptions;

namespace ShockwaveFlash.Types.Sound;

public sealed class SoundFormat
{
    public AudioCompression Compression { get; set; }

    public ushort SampleRate { get; set; }

    public bool IsStereo { get; set; }

    public bool Is16Bit { get; set; }

    public SoundFormat(AudioCompression compression, ushort sampleRate, bool isStereo, bool is16Bit)
    {
        Compression = compression;
        SampleRate = sampleRate;
        IsStereo = isStereo;
        Is16Bit = is16Bit;
    }

    public static SoundFormat Decode(MemoryReader reader)
    {
        var flags = reader.ReadUInt8();
        var compression = (AudioCompression)(flags >> 4);
        ushort sampleRate = ((flags & 12) >> 2) switch
        {
            0 => 5512,
            1 => 11025,
            2 => 22050,
            3 => 44100,
            _ => throw new SwfFormatException("Invalid sound sample-rate code.")
        };
        var is16Bit = (flags & 2) is not 0;
        var isStereo = (flags & 1) is not 0;

        return new SoundFormat(compression, sampleRate, isStereo, is16Bit);
    }

    public void Encode(MemoryWriter writer)
    {
        var sampleRateBits = SampleRate switch
        {
            5512 => 0,
            11025 => 1,
            22050 => 2,
            44100 => 3,
            _ => throw new SwfFormatException("Not supported sample rate.")
        };

        var flags = (byte)(((byte)Compression << 4) | (sampleRateBits << 2) | (Is16Bit ? 2 : 0) | (IsStereo ? 1 : 0));

        writer.WriteUInt8(flags);
    }
}
