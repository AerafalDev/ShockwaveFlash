using CsCheck;
using ShockwaveFlash.IO.Binary;
using ShockwaveFlash.IO.Bits;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Tests;

public sealed class PropertyTests
{
    [Fact]
    public void Fixed16_round_trips_exactly()
    {
        Gen.Int.Sample(raw =>
        {
            var writer = new MemoryWriter();
            writer.WriteFixed(new Fixed16(raw));
            return new MemoryReader(writer.WrittenMemory).ReadFixed().Raw == raw;
        });
    }

    [Fact]
    public void Fixed8_round_trips_exactly()
    {
        Gen.Short.Sample(raw =>
        {
            var writer = new MemoryWriter();
            writer.WriteFixed8(new Fixed8(raw));
            return new MemoryReader(writer.WrittenMemory).ReadFixed8().Raw == raw;
        });
    }

    [Fact]
    public void Unsigned_bit_fields_round_trip()
    {
        Gen.Select(Gen.Int[1, 32], Gen.UInt).Sample((nBits, value) =>
        {
            var mask = nBits is 32 ? uint.MaxValue : (1u << nBits) - 1;
            var masked = value & mask;

            var writer = new MemoryWriter();
            var bitWriter = new BitWriter();
            bitWriter.WriteUBits(writer, masked, nBits);
            bitWriter.Flush(writer);

            var reader = new MemoryReader(writer.WrittenMemory);
            var bitReader = new BitReader();
            return bitReader.ReadUBits(reader, nBits) == masked;
        });
    }

    [Fact]
    public void Signed_bit_fields_round_trip()
    {
        Gen.Select(Gen.Int[1, 32], Gen.Int).Sample((nBits, value) =>
        {
            var shift = 32 - nBits;
            var clamped = (value << shift) >> shift;

            var writer = new MemoryWriter();
            var bitWriter = new BitWriter();
            bitWriter.WriteSBits(writer, clamped, nBits);
            bitWriter.Flush(writer);

            var reader = new MemoryReader(writer.WrittenMemory);
            var bitReader = new BitReader();
            return bitReader.ReadSBits(reader, nBits) == clamped;
        });
    }

    [Fact]
    public void Rectangle_round_trips()
    {
        Gen.Int[-(1 << 29), 1 << 29].Array[4].Sample(c =>
        {
            var rectangle = new Rectangle(c[0], c[1], c[2], c[3]);

            var writer = new MemoryWriter();
            rectangle.Encode(writer);

            return Rectangle.Decode(new MemoryReader(writer.WrittenMemory)) == rectangle;
        });
    }

    [Fact]
    public void Matrix_round_trips_with_exact_fixed_point()
    {
        Gen.Int[-(1 << 29), 1 << 29].Array[6].Sample(v =>
        {
            var matrix = new Matrix(
                new FixedPoint2(new Fixed16(v[0]), new Fixed16(v[1])),
                new FixedPoint2(new Fixed16(v[2]), new Fixed16(v[3])),
                new Point(v[4], v[5]));

            var writer = new MemoryWriter();
            matrix.Encode(writer);

            return Matrix.Decode(new MemoryReader(writer.WrittenMemory)) == matrix;
        });
    }

    [Fact]
    public void ColorTransform_rgba_round_trips()
    {
        Gen.Int[-(1 << 13), 1 << 13].Array[8].Sample(t =>
        {
            var transform = new ColorTransform(t[0], t[1], t[2], t[3], t[4], t[5], t[6], t[7]);

            var writer = new MemoryWriter();
            transform.EncodeRgba(writer);

            return ColorTransform.DecodeRgba(new MemoryReader(writer.WrittenMemory)) == transform;
        });
    }

    [Fact]
    public void Color_rgba_round_trips()
    {
        Gen.Byte.Array[4].Sample(b =>
        {
            var color = new Color(b[0], b[1], b[2], b[3]);

            var writer = new MemoryWriter();
            color.EncodeRgba(writer);

            return Color.DecodeRgba(new MemoryReader(writer.WrittenMemory)) == color;
        });
    }

    [Fact]
    public void EncodedU32_round_trips_every_uint()
    {
        Gen.UInt.Sample(static value =>
        {
            var writer = new MemoryWriter();
            writer.WriteEncodedU32(value);
            return new MemoryReader(writer.WrittenMemory).ReadEncodedU32() == value;
        });
    }

    [Fact]
    public void UInt24_round_trips_every_value_in_range()
    {
        Gen.UInt[0, 0xFFFFFF].Sample(static value =>
        {
            var writer = new MemoryWriter();
            writer.WriteUInt24(value);
            return new MemoryReader(writer.WrittenMemory).ReadUInt24() == value;
        });
    }

    [Fact]
    public void FixedPoint2_round_trips_with_exact_fixed_point()
    {
        Gen.Int.Array[2].Sample(static raw =>
        {
            var point = new FixedPoint2(new Fixed16(raw[0]), new Fixed16(raw[1]));

            var writer = new MemoryWriter();
            writer.WriteFixedPoint2(point);

            return new MemoryReader(writer.WrittenMemory).ReadFixedPoint2() == point;
        });
    }

    [Fact]
    public void Null_terminated_strings_round_trip()
    {
        Gen.String.Where(static value => !value.Contains('\0')).Sample(static value =>
        {
            var writer = new MemoryWriter();
            writer.WriteNullTerminatedString(value);
            return new MemoryReader(writer.WrittenMemory).ReadNullTerminatedString() == value;
        });
    }
}
