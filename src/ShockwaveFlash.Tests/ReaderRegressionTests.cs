using ShockwaveFlash;
using ShockwaveFlash.IO.Binary;
using ShockwaveFlash.IO.Bits;
using ShockwaveFlash.Tags.Control;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class ReaderRegressionTests
{
    [Fact]
    public void Disassemble_decompresses_a_zlib_compressed_movie()
    {
        var compressed = SwfTestData.ToZlibCompressed(SwfTestData.MinimalUncompressed());

        var swf = ShockwaveFlashFile.Disassemble(compressed);

        swf.Header.Compression.ShouldBe(ShockwaveFlashCompression.ZLib);
        swf.Header.FrameRate.ToSingle().ShouldBe(24f);
        swf.Header.FrameCount.ShouldBe((ushort)1);
        swf.Tags.Count.ShouldBe(3);
        swf.Tags[0].ShouldBeOfType<SetBackgroundColorTag>();
    }

    [Fact]
    public void ReadUBits_returns_zero_for_zero_bits_without_consuming()
    {
        var reader = new MemoryReader(new byte[] { 0xFF });
        var bits = new BitReader();

        bits.ReadUBits(reader, 0).ShouldBe(0u);
        reader.Position.ShouldBe(0);
    }

    [Fact]
    public void ReadMemory_allows_zero_length_slices()
    {
        var reader = new MemoryReader(new byte[] { 1, 2, 3 });

        reader.ReadMemory(0).Length.ShouldBe(0);
        reader.Position.ShouldBe(0);
    }
}
