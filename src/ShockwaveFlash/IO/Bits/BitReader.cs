// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace ShockwaveFlash.IO.Bits;

public struct BitReader
{
    private uint _bits;
    private int _position;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadIBits(ref SpanReader reader, int nBits)
    {
        return (int)ReadUBits(ref reader, nBits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadSBits(ref SpanReader reader, int nBits)
    {
        var raw = (int)ReadUBits(ref reader, nBits);
        var shift = 32 - nBits;

        return (raw << shift) >> shift;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFBits(ref SpanReader reader, int nBits)
    {
        return ReadSBits(ref reader, nBits) / 65536f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBit(ref SpanReader reader)
    {
        return ReadUBits(ref reader, 1) is 1;
    }

    public uint ReadUBits(ref SpanReader reader, int nBits)
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
