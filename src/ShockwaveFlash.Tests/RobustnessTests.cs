using ShockwaveFlash;
using ShockwaveFlash.Exceptions;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class RobustnessTests
{
    [Fact]
    public void Truncated_movie_throws_a_typed_swf_exception()
    {
        var truncated = SwfTestData.MinimalUncompressed()[..10];

        Should.Throw<SwfException>(() => ShockwaveFlashFile.Disassemble(truncated));
    }

    [Fact]
    public void A_tag_length_running_past_the_buffer_throws_truncated()
    {
        byte[] body =
        [
            0x08, 0x00, 0x00, 0x18, 0x01, 0x00,
            0x3F, 0x00, 0xFF, 0xFF, 0x7F, 0x00,
        ];

        var file = new byte[8 + body.Length];
        file[0] = (byte)'F';
        file[1] = (byte)'W';
        file[2] = (byte)'S';
        file[3] = 6;
        file[4] = (byte)file.Length;
        body.CopyTo(file, 8);

        Should.Throw<SwfTruncatedException>(() => ShockwaveFlashFile.Disassemble(file));
    }

    [Fact]
    public void Corrupt_zlib_body_throws_a_compression_exception()
    {
        byte[] file =
        [
            (byte)'C', (byte)'W', (byte)'S', 6,
            0x40, 0x00, 0x00, 0x00,
            0xDE, 0xAD, 0xBE, 0xEF, 0x01, 0x02, 0x03, 0x04,
        ];

        Should.Throw<SwfCompressionException>(() => ShockwaveFlashFile.Disassemble(file));
    }
}
