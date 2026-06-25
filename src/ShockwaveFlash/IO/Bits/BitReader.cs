using System.Runtime.CompilerServices;

namespace ShockwaveFlash.IO.Bits;

public sealed class BitReader
{
    private ulong _bits;
    private int _position;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        _bits = 0;
        _position = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadIBits(MemoryReader reader, int nBits)
    {
        return (int)ReadUBits(reader, nBits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadSBits(MemoryReader reader, int nBits)
    {
        var raw = (int)ReadUBits(reader, nBits);
        var shift = 32 - nBits;

        return (raw << shift) >> shift;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFBits(MemoryReader reader, int nBits)
    {
        return ReadSBits(reader, nBits) / 65536f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBit(MemoryReader reader)
    {
        return ReadUBits(reader, 1) is 1;
    }

    public uint ReadUBits(MemoryReader reader, int nBits)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(nBits);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(nBits, 32);

        if (nBits is 0)
            return 0;

        while (_position < nBits)
        {
            _bits = (_bits << 8) | reader.ReadUInt8();
            _position += 8;
        }

        _position -= nBits;

        return (uint)((_bits >> _position) & (nBits is 32 ? uint.MaxValue : (1u << nBits) - 1));
    }
}
