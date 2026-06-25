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

/// <summary>
/// Characterization tests pinning reader behaviour after the
/// <c>SpanReader</c> -> <c>MemoryReader</c> (ReadOnlyMemory, no-ref) migration.
/// </summary>
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
        // The deferred ReadOnlyMemory payload is self-contained: DecodeActions
        // takes only the SWF version, no reader/buffer (the migration's goal).
        var swf = ShockwaveFlashFile.Disassemble(SwfTestData.WithDoAction());

        var doAction = swf.Tags.OfType<DoActionTag>().ShouldHaveSingleItem();

        var actions = doAction.DecodeActions(swf.Header.Version);

        actions.Count.ShouldBe(3);
        actions[0].Opcode.ShouldBe(Actions.Avm1.ActionOpcode.Play);
        actions[1].Opcode.ShouldBe(Actions.Avm1.ActionOpcode.Stop);
        actions[2].Opcode.ShouldBe(Actions.Avm1.ActionOpcode.End);
    }

    [Fact]
    public void Disassemble_rejects_an_unknown_tag_code()
    {
        // RECORDHEADER for tag code 1023 (undefined), length 0.
        byte[] body =
        [
            0x08, 0x00, 0x00, 0x18, 0x01, 0x00, // header fields
            0xC0, 0xFF,                         // code 1023, length 0
            0x00, 0x00,                         // End
        ];

        var file = new byte[8 + body.Length];
        file[0] = (byte)'F';
        file[1] = (byte)'W';
        file[2] = (byte)'S';
        file[3] = 6;
        file[4] = (byte)file.Length;
        body.CopyTo(file, 8);

        Should.Throw<NotSupportedException>(() => ShockwaveFlashFile.Disassemble(file));
    }
}
