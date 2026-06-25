using System.Runtime.CompilerServices;

namespace ShockwaveFlash.IO.Bits;

public sealed class BitWriter
{
    private ulong _accumulator;
    private int _count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUBits(MemoryWriter writer, uint value, int nBits)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(nBits);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(nBits, 32);

        if (nBits is 0)
            return;

        var mask = nBits is 32 ? uint.MaxValue : (1u << nBits) - 1;

        _accumulator = (_accumulator << nBits) | (value & mask);
        _count += nBits;

        while (_count >= 8)
        {
            _count -= 8;
            writer.WriteUInt8((byte)(_accumulator >> _count));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSBits(MemoryWriter writer, int value, int nBits)
    {
        WriteUBits(writer, (uint)value, nBits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFBits(MemoryWriter writer, float value, int nBits)
    {
        WriteSBits(writer, (int)Math.Round(value * 65536.0), nBits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBit(MemoryWriter writer, bool value)
    {
        WriteUBits(writer, value ? 1u : 0u, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Flush(MemoryWriter writer)
    {
        if (_count is 0)
            return;

        writer.WriteUInt8((byte)(_accumulator << (8 - _count)));
        _accumulator = 0;
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        _accumulator = 0;
        _count = 0;
    }

    public static int SignedBitsNeeded(int value)
    {
        var n = 1;

        while (n < 32)
        {
            var min = -(1 << (n - 1));
            var max = (1 << (n - 1)) - 1;

            if (value >= min && value <= max)
                return n;

            n++;
        }

        return 32;
    }

    public static int UnsignedBitsNeeded(uint value)
    {
        if (value is 0)
            return 0;

        var n = 0;

        while (value is not 0)
        {
            value >>= 1;
            n++;
        }

        return n;
    }
}
