// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace ShockwaveFlash.IO.Binary;

public sealed class MemoryWriter
{
    private const int Mask10000000 = 128;
    private const int Mask01111111 = 127;
    private const int ChunkBitSize = 7;

    private byte[] _buffer;

    private int _position;

    public int Position
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _position;
    }

    public ReadOnlyMemory<byte> WrittenMemory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer.AsMemory(0, _position);
    }

    public MemoryWriter()
        : this(256)
    {
    }

    public MemoryWriter(int capacity)
    {
        _buffer = new byte[capacity < 1 ? 1 : capacity];
        _position = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt8(byte value)
    {
        Reserve(sizeof(byte))[0] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt8(sbyte value)
    {
        Reserve(sizeof(sbyte))[0] = (byte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBoolean(bool value)
    {
        Reserve(sizeof(bool))[0] = (byte)(value ? 1 : 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt16(ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(Reserve(sizeof(ushort)), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt16(short value)
    {
        BinaryPrimitives.WriteInt16LittleEndian(Reserve(sizeof(short)), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt24(uint value)
    {
        var span = Reserve(3);
        span[0] = (byte)value;
        span[1] = (byte)(value >> 8);
        span[2] = (byte)(value >> 16);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt24(int value)
    {
        WriteUInt24((uint)(value & 0xFFFFFF));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt32(uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(Reserve(sizeof(uint)), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt32(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(Reserve(sizeof(int)), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt64(ulong value)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(Reserve(sizeof(ulong)), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt64(long value)
    {
        BinaryPrimitives.WriteInt64LittleEndian(Reserve(sizeof(long)), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFloat16(float value)
    {
        BinaryPrimitives.WriteHalfLittleEndian(Reserve(sizeof(short)), (Half)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFloat32(float value)
    {
        BinaryPrimitives.WriteSingleLittleEndian(Reserve(sizeof(float)), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFloat64(double value)
    {
        BinaryPrimitives.WriteDoubleLittleEndian(Reserve(sizeof(double)), value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFloat64Avm1(double value)
    {
        var bits = (ulong)BitConverter.DoubleToInt64Bits(value);
        WriteUInt32((uint)(bits >> 32));
        WriteUInt32((uint)bits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUFixed8(float value)
    {
        WriteUInt16((ushort)Math.Round(value * 256.0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixed8(float value)
    {
        WriteInt16((short)Math.Round(value * 256.0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUFixed(float value)
    {
        WriteUInt32((uint)Math.Round(value * 65536.0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixed(float value)
    {
        WriteInt32((int)Math.Round(value * 65536.0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNullTerminatedString(string value)
    {
        WriteNullTerminatedString(value, Encoding.UTF8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNullTerminatedString(string value, Encoding encoding)
    {
        WriteBytes(encoding.GetBytes(value));
        WriteUInt8(0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLengthPrefixedString(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteUInt8((byte)bytes.Length);
        WriteBytes(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVector2(Vector2 value)
    {
        WriteFixed(value.X);
        WriteFixed(value.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteEncodedU32(uint value)
    {
        do
        {
            var b = (byte)(value & Mask01111111);
            value >>= ChunkBitSize;

            if (value is not 0)
                b |= Mask10000000;

            WriteUInt8(b);
        } while (value is not 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(ReadOnlySpan<byte> value)
    {
        value.CopyTo(Reserve(value.Length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteMemory(ReadOnlyMemory<byte> value)
    {
        WriteBytes(value.Span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ToArray()
    {
        return _buffer.AsSpan(0, _position).ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Span<byte> Reserve(int count)
    {
        if (_position + count > _buffer.Length)
            Grow(_position + count);

        var span = _buffer.AsSpan(_position, count);
        _position += count;
        return span;
    }

    private void Grow(int required)
    {
        var capacity = _buffer.Length * 2;

        while (capacity < required)
            capacity *= 2;

        Array.Resize(ref _buffer, capacity);
    }
}
