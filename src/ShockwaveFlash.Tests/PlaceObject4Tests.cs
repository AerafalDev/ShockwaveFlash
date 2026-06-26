using ShockwaveFlash;
using ShockwaveFlash.IO.Binary;
using ShockwaveFlash.Tags;
using ShockwaveFlash.Tags.DisplayList;
using ShockwaveFlash.Types.DisplayList;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class PlaceObject4Tests
{
    [Fact]
    public void Decodes_placement_fields_and_keeps_the_trailing_amf_data()
    {
        byte[] amf = [0x0A, 0x0B, 0x01, 0x09, 0x6E, 0x61, 0x6D, 0x65];

        var body = new MemoryWriter();
        body.WriteUInt16((ushort)PlaceObjectFlags.HasCharacter);
        body.WriteUInt16(3);
        body.WriteUInt16(42);
        body.WriteBytes(amf);

        var tag = PlaceObject4Tag.Decode(
            new MemoryReader(body.WrittenMemory),
            new TagMetadata(TagCode.PlaceObject4, 0, (int)body.Position),
            swfVersion: 10);

        tag.Placement.Depth.ShouldBe((ushort)3);
        tag.Placement.Action.ShouldBeOfType<PlaceObjectAction.PlaceObjectActionPlace>().Id.ShouldBe((ushort)42);
        tag.AmfData.ToArray().ShouldBe(amf);
    }

    [Fact]
    public void Round_trips_inside_a_movie()
    {
        byte[] amf = [0x0A, 0x0B, 0x01, 0x00];

        var place = new MemoryWriter();
        place.WriteUInt16((ushort)PlaceObjectFlags.HasCharacter);
        place.WriteUInt16(1);
        place.WriteUInt16(7);
        place.WriteBytes(amf);

        var body = new MemoryWriter();
        body.WriteBytes([0x08, 0x00, 0x00, 0x18, 0x01, 0x00]);
        WriteTag(body, TagCode.SetBackgroundColor, [0x00, 0x00, 0x00]);
        WriteTag(body, TagCode.PlaceObject4, place.WrittenMemory.Span);
        WriteTag(body, TagCode.ShowFrame, []);
        WriteTag(body, TagCode.End, []);

        var swf = ShockwaveFlashFile.Disassemble(Wrap(body.WrittenMemory.Span));
        swf.Tags.OfType<PlaceObject4Tag>().ShouldHaveSingleItem().AmfData.ToArray().ShouldBe(amf);

        var reparsed = ShockwaveFlashFile.Disassemble(swf.Assemble());
        reparsed.Tags.OfType<PlaceObject4Tag>().ShouldHaveSingleItem().AmfData.ToArray().ShouldBe(amf);
    }

    private static void WriteTag(MemoryWriter body, TagCode code, ReadOnlySpan<byte> payload)
    {
        var header = ((ushort)code << 6) | Math.Min(payload.Length, 0x3F);
        body.WriteUInt16((ushort)header);

        if (payload.Length >= 0x3F)
            body.WriteInt32(payload.Length);

        body.WriteBytes(payload);
    }

    private static byte[] Wrap(ReadOnlySpan<byte> body)
    {
        var fileLength = 8 + body.Length;
        var file = new byte[fileLength];
        file[0] = (byte)'F';
        file[1] = (byte)'W';
        file[2] = (byte)'S';
        file[3] = 10;
        file[4] = (byte)(fileLength & 0xFF);
        file[5] = (byte)((fileLength >> 8) & 0xFF);
        body.CopyTo(file.AsSpan(8));

        return file;
    }
}
