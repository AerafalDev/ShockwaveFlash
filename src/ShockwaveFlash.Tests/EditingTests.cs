using ShockwaveFlash;
using ShockwaveFlash.Tags.Control;
using ShockwaveFlash.Types;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class EditingTests
{
    private static ShockwaveFlashFile Movie()
    {
        return ShockwaveFlashFile.Disassemble(SwfTestData.MinimalUncompressed());
    }

    [Fact]
    public void Appending_a_tag_round_trips()
    {
        var swf = Movie();
        var count = swf.Tags.Count;
        var background = (SetBackgroundColorTag)swf.Tags[0];

        swf.Tags.Add(new SetBackgroundColorTag(background.Metadata, background.BackgroundColor));

        swf.Tags.Count.ShouldBe(count + 1);
        ShockwaveFlashFile.Disassemble(swf.Assemble()).Tags.Count.ShouldBe(count + 1);
    }

    [Fact]
    public void Replacing_a_tag_in_place_is_reflected_on_encode()
    {
        var swf = Movie();
        var background = (SetBackgroundColorTag)swf.Tags[0];

        swf.Tags[0] = new SetBackgroundColorTag(background.Metadata, new Color(1, 2, 3, 255));

        var reparsed = ShockwaveFlashFile.Disassemble(swf.Assemble());
        ((SetBackgroundColorTag)reparsed.Tags[0]).BackgroundColor.ShouldBe(new Color(1, 2, 3, 255));
    }

    [Fact]
    public void Mutating_a_tag_in_place_is_reflected_on_encode()
    {
        var swf = Movie();
        ((SetBackgroundColorTag)swf.Tags[0]).BackgroundColor = new Color(9, 8, 7, 255);

        var reparsed = ShockwaveFlashFile.Disassemble(swf.Assemble());
        ((SetBackgroundColorTag)reparsed.Tags[0]).BackgroundColor.ShouldBe(new Color(9, 8, 7, 255));
    }

    [Fact]
    public void Removing_a_tag_round_trips()
    {
        var swf = Movie();
        var count = swf.Tags.Count;

        swf.Tags.RemoveAt(swf.Tags.Count - 1);

        swf.Tags.Count.ShouldBe(count - 1);
        ShockwaveFlashFile.Disassemble(swf.Assemble()).Tags.Count.ShouldBe(count - 1);
    }
}
