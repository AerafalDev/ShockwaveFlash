using ShockwaveFlash;
using ShockwaveFlash.Exceptions;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class CompressionTests
{
    [Fact]
    public void Lzma_with_an_oversized_declared_length_is_capped_instead_of_allocating()
    {
        byte[] file =
        [
            (byte)'Z', (byte)'W', (byte)'S', 13,
            0xFF, 0xFF, 0xFF, 0x7F,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        ];

        Should.Throw<SwfCompressionException>(() => ShockwaveFlashFile.Disassemble(file));
    }

    [Fact]
    public void Lzma_compressed_movie_round_trips_to_an_equal_model()
    {
        var source = ShockwaveFlashFile.Disassemble(SwfTestData.MinimalUncompressed());
        var lzma = new ShockwaveFlashFile(source.Header with { Compression = ShockwaveFlashCompression.Lzma }, source.Tags);

        var bytes = lzma.Assemble();
        bytes.Span[0].ShouldBe((byte)'Z');

        var reparsed = ShockwaveFlashFile.Disassemble(bytes);
        reparsed.Header.Compression.ShouldBe(ShockwaveFlashCompression.Lzma);
        reparsed.Tags.Count.ShouldBe(source.Tags.Count);

        for (var i = 0; i < source.Tags.Count; i++)
            DeepEquality.Equals(source.Tags[i], reparsed.Tags[i], $"Tags[{i}]", out var where).ShouldBeTrue(where);
    }

    [Fact]
    public void A_real_lzma_swf_decompresses_and_round_trips()
    {
        var path = LocateLzmaSwf();

        if (path is null)
            return;

        var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));
        swf.Header.Compression.ShouldBe(ShockwaveFlashCompression.Lzma);
        swf.Tags.Count.ShouldBeGreaterThan(0);

        var reparsed = ShockwaveFlashFile.Disassemble(swf.Assemble());
        reparsed.Tags.Count.ShouldBe(swf.Tags.Count);
    }

    private static string? LocateLzmaSwf()
    {
        var dir = AppContext.BaseDirectory;

        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "context", "ruffle-master", "swf", "tests", "swfs", "lzma.swf");

            if (File.Exists(candidate))
                return candidate;

            dir = Path.GetDirectoryName(dir);
        }

        return null;
    }
}
