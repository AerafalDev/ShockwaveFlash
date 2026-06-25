// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash;
using ShockwaveFlash.Tags.Action;
using ShockwaveFlash.Tags.Control;
using ShockwaveFlash.Tags.DisplayList;
using ShockwaveFlash.Types;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class SwfReaderTests
{
    [Fact]
    public void Disassemble_decodes_the_movie_header()
    {
        var swf = ShockwaveFlashFile.Disassemble(SwfTestData.MinimalUncompressed());

        swf.Header.Compression.ShouldBe(ShockwaveFlashCompression.None);
        swf.Header.Version.ShouldBe((byte)6);
        swf.Header.FrameRate.ShouldBe(24f);
        swf.Header.FrameCount.ShouldBe((ushort)1);
        swf.Header.FileLength.ShouldBe(23);
    }

    [Fact]
    public void Disassemble_decodes_the_tag_stream_in_order()
    {
        var swf = ShockwaveFlashFile.Disassemble(SwfTestData.MinimalUncompressed());

        swf.Tags.Count.ShouldBe(3);
        swf.Tags[0].ShouldBeOfType<SetBackgroundColorTag>()
            .BackgroundColor.ShouldBe(new Color((byte)255, (byte)0, (byte)0, (byte)255));
        swf.Tags[1].ShouldBeOfType<ShowFrameTag>();
        swf.Tags[2].ShouldBeOfType<EndTag>();
    }

    [Fact]
    public void DoAction_payload_decodes_without_resupplying_the_buffer()
    {
        var swf = ShockwaveFlashFile.Disassemble(SwfTestData.WithDoAction());

        var doAction = swf.Tags.OfType<DoActionTag>().ShouldHaveSingleItem();

        doAction.Data.ToArray().ShouldBe(new byte[] { 0x06, 0x07, 0x00 });
    }

    [Fact]
    public void Disassemble_preserves_an_unknown_tag_as_raw_bytes()
    {
        byte[] body =
        [
            0x08, 0x00, 0x00, 0x18, 0x01, 0x00,
            0xC2, 0xFF, 0xAB, 0xCD,
            0x00, 0x00,
        ];

        var file = new byte[8 + body.Length];
        file[0] = (byte)'F';
        file[1] = (byte)'W';
        file[2] = (byte)'S';
        file[3] = 6;
        file[4] = (byte)file.Length;
        body.CopyTo(file, 8);

        var swf = ShockwaveFlashFile.Disassemble(file);

        var unknown = swf.Tags.OfType<Tags.UnknownTag>().ShouldHaveSingleItem();
        ((ushort)unknown.Metadata.Code).ShouldBe((ushort)1023);
        unknown.Data.ToArray().ShouldBe(new byte[] { 0xAB, 0xCD });

        swf.Assemble().ToArray().ShouldBe(file);
    }
}
