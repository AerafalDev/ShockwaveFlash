// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using ShockwaveFlash.IO.Buffers;
using ShockwaveFlash.IO.Extensions;

namespace ShockwaveFlash.IO.Binary;

public ref struct SpanReader
{
    private const int Mask10000000 = 128;
    private const int Mask01111111 = 127;
    private const int ChunkBitSize = 7;

    private readonly ReadOnlySpan<byte> _buffer;

    private int _position;

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer.Length;
    }

    public int Position
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _position;
    }

    public int Remaining
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer.Length - _position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SpanReader(ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer;
        _position = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadUInt8()
    {
        return ReadSpan(sizeof(byte))[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte ReadInt8()
    {
        return (sbyte)ReadSpan(sizeof(sbyte))[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBoolean()
    {
        return ReadSpan(sizeof(bool))[0] is not 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadUInt16()
    {
        return BinaryPrimitives.ReadUInt16LittleEndian(ReadSpan(sizeof(ushort)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadInt16()
    {
        return BinaryPrimitives.ReadInt16LittleEndian(ReadSpan(sizeof(short)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt24()
    {
        return BinaryPrimitives.ReadUInt24LittleEndian(ReadSpan(3));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt24()
    {
        return BinaryPrimitives.ReadInt24LittleEndian(ReadSpan(3));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt32()
    {
        return BinaryPrimitives.ReadUInt32LittleEndian(ReadSpan(sizeof(uint)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt32()
    {
        return BinaryPrimitives.ReadInt32LittleEndian(ReadSpan(sizeof(int)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ReadUInt64()
    {
        return BinaryPrimitives.ReadUInt64LittleEndian(ReadSpan(sizeof(ulong)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadInt64()
    {
        return BinaryPrimitives.ReadInt64LittleEndian(ReadSpan(sizeof(long)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFloat16()
    {
        return (float)BinaryPrimitives.ReadHalfLittleEndian(ReadSpan(sizeof(short)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFloat32()
    {
        return BinaryPrimitives.ReadSingleLittleEndian(ReadSpan(sizeof(float)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadFloat64()
    {
        return BinaryPrimitives.ReadDoubleLittleEndian(ReadSpan(sizeof(double)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadUFixed8()
    {
        return ReadUInt16() / 256f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFixed8()
    {
        return ReadInt16() / 256f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadUFixed()
    {
        return ReadUInt32() / 65536f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFixed()
    {
        return ReadInt32() / 65536f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadNullTerminatedString()
    {
        var slice = _buffer[_position..];
        var index = slice.IndexOf((byte)0);
        EnsureNullTerminatedStringIndex(index);
        var str = Encoding.UTF8.GetString(slice[..index]);
        _position += index + 1;
        return str;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadLengthPrefixedString()
    {
        return Encoding.UTF8.GetString(ReadSpan(ReadUInt8()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ReadVector2()
    {
        return new Vector2(ReadFixed(), ReadFixed());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadEncodedU32()
    {
        uint result = 0;
        byte b;
        var shift = 0;

        do
        {
            b = ReadUInt8();
            result |= (uint)(b & Mask01111111) << shift;
            shift += ChunkBitSize;
        } while ((b & Mask10000000) is not 0);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SpanSlice Slice(int length)
    {
        EnsureBuffer(length);
        var slice = new SpanSlice(_position, length);
        _position += length;
        return slice;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SpanSlice SliceToEnd()
    {
        return Slice(Remaining);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadSlice(ref readonly SpanSlice slice)
    {
        EnsureSlice(in slice);
        return _buffer.Slice(slice.Offset, slice.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        EnsureBuffer(count);
        _position += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadSpan(int count)
    {
        EnsureBuffer(count);
        var span = _buffer.Slice(_position, count);
        _position += count;
        return span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureSlice(ref readonly SpanSlice slice)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slice.Offset);
        ArgumentOutOfRangeException.ThrowIfNegative(slice.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(slice.Offset + slice.Length, _buffer.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureBuffer(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_position + count, _buffer.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnsureNullTerminatedStringIndex(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
    }
}
