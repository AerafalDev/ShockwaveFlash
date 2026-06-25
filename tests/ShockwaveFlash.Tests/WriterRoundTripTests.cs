// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class WriterRoundTripTests
{
    [Fact]
    public void Assemble_reproduces_a_canonical_uncompressed_movie_byte_for_byte()
    {
        var original = SwfTestData.MinimalUncompressed();

        var rebuilt = ShockwaveFlashFile.Disassemble(original).Assemble();

        rebuilt.ToArray().ShouldBe(original);
    }

    [Fact]
    public void Assemble_reproduces_a_do_action_movie_byte_for_byte()
    {
        var original = SwfTestData.WithDoAction();

        var rebuilt = ShockwaveFlashFile.Disassemble(original).Assemble();

        rebuilt.ToArray().ShouldBe(original);
    }

    [Fact]
    public void Read_write_is_a_fixed_point_for_a_compressed_movie()
    {
        var compressed = SwfTestData.ToZlibCompressed(SwfTestData.MinimalUncompressed());

        var first = ShockwaveFlashFile.Disassemble(compressed).Assemble();
        var second = ShockwaveFlashFile.Disassemble(first).Assemble();

        first.ToArray().ShouldBe(second.ToArray());
    }

    [Fact]
    public void Assembled_movie_reparses_to_an_equivalent_model()
    {
        var swf = ShockwaveFlashFile.Disassemble(SwfTestData.MinimalUncompressed());

        var reparsed = ShockwaveFlashFile.Disassemble(swf.Assemble());

        reparsed.Header.Compression.ShouldBe(swf.Header.Compression);
        reparsed.Header.Version.ShouldBe(swf.Header.Version);
        reparsed.Header.FrameSize.ShouldBe(swf.Header.FrameSize);
        reparsed.Header.FrameRate.ShouldBe(swf.Header.FrameRate);
        reparsed.Header.FrameCount.ShouldBe(swf.Header.FrameCount);
        reparsed.Tags.Count.ShouldBe(swf.Tags.Count);
    }
}
