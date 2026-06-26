using ShockwaveFlash;
using ShockwaveFlash.Rendering;
using ShockwaveFlash.Tags;
using ShockwaveFlash.Tags.Font;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Shape;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class FontTests
{
    [Fact]
    public void DefineFont_v1_is_indexed_with_codes_from_DefineFontInfo()
    {
        var header = new ShockwaveFlashHeader(ShockwaveFlashCompression.None, 6, 0, new Rectangle(0, 0, 0, 0), new Fixed8(0), 1);

        IReadOnlyList<IReadOnlyList<ShapeRecord>> glyphs = [[new EndShapeRecord()], [new EndShapeRecord()]];
        var font = new DefineFontTag(new TagMetadata(TagCode.DefineFont, 0, 0), 5, [4, 4], glyphs);
        var info = new DefineFontInfoTag(new TagMetadata(TagCode.DefineFontInfo, 0, 0), 5, "Font", default, [65, 66]);

        var file = new ShockwaveFlashFile(header, [font, info]);

        var resolved = new SwfRenderer(file).ResolveFont(5);

        resolved.ShouldNotBeNull();
        resolved.Glyphs.Count.ShouldBe(2);
        resolved.EmSquare.ShouldBe(1024f);
        resolved.IndexForCode(65).ShouldBe(0);
        resolved.IndexForCode(66).ShouldBe(1);
        resolved.IndexForCode(99).ShouldBe(-1);
    }
}
