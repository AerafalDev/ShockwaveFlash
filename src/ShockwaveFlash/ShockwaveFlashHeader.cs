using ShockwaveFlash.Types;

namespace ShockwaveFlash;

public readonly record struct ShockwaveFlashHeader(
    ShockwaveFlashCompression Compression,
    byte Version,
    int FileLength,
    Rectangle FrameSize,
    Fixed8 FrameRate,
    ushort FrameCount)
{
    public static ShockwaveFlashHeader Decode(MemoryReader reader, ShockwaveFlashCompression compression, byte version, int fileLength)
    {
        var frameSize = Rectangle.Decode(reader);
        var frameRate = reader.ReadFixed8();
        var frameCount = reader.ReadUInt16();

        return new ShockwaveFlashHeader(compression, version, fileLength, frameSize, frameRate, frameCount);
    }

    public void Encode(MemoryWriter writer)
    {
        FrameSize.Encode(writer);
        writer.WriteFixed8(FrameRate);
        writer.WriteUInt16(FrameCount);
    }
}
