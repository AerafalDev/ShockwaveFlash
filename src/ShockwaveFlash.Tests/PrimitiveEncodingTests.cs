using ShockwaveFlash.IO.Binary;
using ShockwaveFlash.Types;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class PrimitiveEncodingTests
{
    [Fact]
    public void Integers_decode_little_endian()
    {
        new MemoryReader(new byte[] { 0x34, 0x12 }).ReadUInt16().ShouldBe((ushort)0x1234);
        new MemoryReader(new byte[] { 0x56, 0x34, 0x12 }).ReadUInt24().ShouldBe(0x123456u);
        new MemoryReader(new byte[] { 0x78, 0x56, 0x34, 0x12 }).ReadUInt32().ShouldBe(0x12345678u);
        new MemoryReader(new byte[] { 0x00, 0x80 }).ReadInt16().ShouldBe(short.MinValue);
    }

    [Theory]
    [InlineData(0u, new byte[] { 0x00 })]
    [InlineData(127u, new byte[] { 0x7F })]
    [InlineData(128u, new byte[] { 0x80, 0x01 })]
    [InlineData(16383u, new byte[] { 0xFF, 0x7F })]
    [InlineData(uint.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x0F })]
    public void EncodedU32_matches_the_canonical_leb128_layout(uint value, byte[] expected)
    {
        var writer = new MemoryWriter();
        writer.WriteEncodedU32(value);

        writer.WrittenMemory.ToArray().ShouldBe(expected);
        new MemoryReader(expected).ReadEncodedU32().ShouldBe(value);
    }

    [Fact]
    public void Fixed_point_values_decode_at_the_documented_scale()
    {
        new MemoryReader(new byte[] { 0x00, 0x01 }).ReadFixed8().ToSingle().ShouldBe(1f);
        new MemoryReader(new byte[] { 0x00, 0xFF }).ReadFixed8().ToSingle().ShouldBe(-1f);
        new MemoryReader(new byte[] { 0x00, 0x00, 0x01, 0x00 }).ReadFixed().ToSingle().ShouldBe(1f);
        new MemoryReader(new byte[] { 0x80, 0x00, 0x00, 0x00 }).ReadFixed().ToSingle().ShouldBe(0.5f / 256f);
    }

    [Fact]
    public void Color_channels_decode_in_their_documented_orders()
    {
        Color.DecodeRgb(new MemoryReader(new byte[] { 0x12, 0x34, 0x56 }))
            .ShouldBe(new Color(0x12, 0x34, 0x56, 0xFF));

        Color.DecodeRgba(new MemoryReader(new byte[] { 0x12, 0x34, 0x56, 0x78 }))
            .ShouldBe(new Color(0x12, 0x34, 0x56, 0x78));

        Color.DecodeArgb(new MemoryReader(new byte[] { 0x78, 0x12, 0x34, 0x56 }))
            .ShouldBe(new Color(0x12, 0x34, 0x56, 0x78));
    }

    [Fact]
    public void Strings_round_trip_through_both_framings()
    {
        var nullTerminated = new MemoryReader(new byte[] { (byte)'A', (byte)'B', 0x00, 0xFF });
        nullTerminated.ReadNullTerminatedString().ShouldBe("AB");
        nullTerminated.Position.ShouldBe(3);

        new MemoryReader(new byte[] { 0x02, (byte)'A', (byte)'B' }).ReadLengthPrefixedString().ShouldBe("AB");
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(300u)]
    [InlineData(0x12345678u)]
    [InlineData(uint.MaxValue)]
    public void EncodedU32_round_trips(uint value)
    {
        var writer = new MemoryWriter();
        writer.WriteEncodedU32(value);

        new MemoryReader(writer.WrittenMemory).ReadEncodedU32().ShouldBe(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(255)]
    [InlineData(0xFFFFFF)]
    public void UInt24_round_trips(int value)
    {
        var writer = new MemoryWriter();
        writer.WriteUInt24((uint)value);

        new MemoryReader(writer.WrittenMemory).ReadUInt24().ShouldBe((uint)value);
    }
}
