using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using ShockwaveFlash.Exceptions;
using ShockwaveFlash.IO.Extensions;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.IO.Binary;

public sealed class MemoryReader
{
    private const int Mask10000000 = 128;
    private const int Mask01111111 = 127;
    private const int ChunkBitSize = 7;

    private readonly ReadOnlyMemory<byte> _buffer;

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
    public MemoryReader(ReadOnlyMemory<byte> buffer)
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
    public Fixed8 ReadFixed8()
    {
        return new Fixed8(ReadInt16());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadUFixed()
    {
        return ReadUInt32() / 65536f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Fixed16 ReadFixed()
    {
        return new Fixed16(ReadInt32());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FixedPoint2 ReadFixedPoint2()
    {
        return new FixedPoint2(ReadFixed(), ReadFixed());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadNullTerminatedString()
    {
        return ReadNullTerminatedString(Encoding.UTF8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadNullTerminatedString(Encoding encoding)
    {
        var slice = _buffer.Span[_position..];
        var index = slice.IndexOf((byte)0);
        EnsureNullTerminatedStringIndex(index);
        var str = encoding.GetString(slice[..index]);
        _position += index + 1;
        return str;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadLengthPrefixedString()
    {
        return Encoding.UTF8.GetString(ReadSpan(ReadUInt8()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadEncodedU32()
    {
        uint result = 0;
        var shift = 0;

        for (var i = 0; i < 5; i++)
        {
            var b = ReadUInt8();
            result |= (uint)(b & Mask01111111) << shift;

            if ((b & Mask10000000) is 0)
                break;

            shift += ChunkBitSize;
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<byte> ReadMemory(int count)
    {
        EnsureBuffer(count);
        var memory = _buffer.Slice(_position, count);
        _position += count;
        return memory;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<byte> ReadMemoryToEnd()
    {
        return ReadMemory(Remaining);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        EnsureBuffer(count);
        _position += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<byte> ReadSpan(int count)
    {
        EnsureBuffer(count);
        var span = _buffer.Span.Slice(_position, count);
        _position += count;
        return span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureBuffer(int count)
    {
        if (count < 0)
            throw new SwfFormatException($"Negative read length {count} at position {_position}.");

        if (count > _buffer.Length - _position)
            throw new SwfTruncatedException($"Tried to read {count} bytes at position {_position} but only {_buffer.Length - _position} remain.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnsureNullTerminatedStringIndex(int index)
    {
        if (index < 0)
            throw new SwfTruncatedException("Reached end of buffer before a null string terminator.");
    }
}
