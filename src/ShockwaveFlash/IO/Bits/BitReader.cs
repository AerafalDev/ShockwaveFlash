// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace ShockwaveFlash.IO.Bits;

/// <summary>
/// MSB-first bit reader that drives a <see cref="MemoryReader"/>.
/// Reference type so its accumulator state is shared without <c>ref</c>; call
/// <see cref="Reset"/> to discard buffered bits at a byte boundary.
/// </summary>
public sealed class BitReader
{
    private uint _bits;
    private int _position;

    /// <summary>Discards any buffered bits, realigning to the next whole byte.</summary>
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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nBits);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(nBits, 32);

        while (_position < nBits)
        {
            _bits = (_bits << 8) | reader.ReadUInt8();
            _position += 8;
        }

        _position -= nBits;

        return (_bits >> _position) & (nBits is 32 ? uint.MaxValue : (1u << nBits) - 1);
    }
}
