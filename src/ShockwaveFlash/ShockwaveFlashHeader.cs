using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using ShockwaveFlash.Types;

namespace ShockwaveFlash;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Sequential)]
public readonly struct ShockwaveFlashHeader :
    IEquatable<ShockwaveFlashHeader>,
    IEqualityOperators<ShockwaveFlashHeader, ShockwaveFlashHeader, bool>
{
    public readonly ShockwaveFlashCompression Compression;

    public readonly byte Version;

    public readonly int FileLength;

    public readonly Rectangle FrameSize;

    public readonly Fixed8 FrameRate;

    public readonly ushort FrameCount;

    public ShockwaveFlashHeader(ShockwaveFlashCompression compression, byte version, int fileLength, Rectangle frameSize, Fixed8 frameRate, ushort frameCount)
    {
        Compression = compression;
        Version = version;
        FileLength = fileLength;
        FrameSize = frameSize;
        FrameRate = frameRate;
        FrameCount = frameCount;
    }

    public bool Equals(ShockwaveFlashHeader other)
    {
        return Compression == other.Compression &&
               Version == other.Version &&
               FileLength == other.FileLength &&
               FrameSize.Equals(other.FrameSize) &&
               FrameRate.Equals(other.FrameRate) &&
               FrameCount == other.FrameCount;
    }

    public override bool Equals(object? obj)
    {
        return obj is ShockwaveFlashHeader other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Compression, Version, FileLength, FrameSize, FrameRate, FrameCount);
    }

    public override string ToString()
    {
        return $"Header(Compression: {Compression}, Version: {Version}, FileLength: {FileLength}, FrameSize: {FrameSize}, FrameRate: {FrameRate}, FrameCount: {FrameCount})";
    }

    public static bool operator ==(ShockwaveFlashHeader left, ShockwaveFlashHeader right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShockwaveFlashHeader left, ShockwaveFlashHeader right)
    {
        return !left.Equals(right);
    }

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
