using ShockwaveFlash;
using ShockwaveFlash.Tags;
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

    private static SetBackgroundColorTag CloneBackground(Tag tag)
    {
        var background = (SetBackgroundColorTag)tag;
        return new SetBackgroundColorTag(background.Metadata, background.BackgroundColor);
    }

    [Fact]
    public void AppendTag_adds_at_the_end_and_round_trips()
    {
        var swf = Movie();
        var edited = swf.AppendTag(CloneBackground(swf.Tags[0]));

        edited.Tags.Count.ShouldBe(swf.Tags.Count + 1);
        ShockwaveFlashFile.Disassemble(edited.Assemble()).Tags.Count.ShouldBe(edited.Tags.Count);
    }

    [Fact]
    public void InsertTag_places_the_tag_at_the_index()
    {
        var swf = Movie();
        var clone = CloneBackground(swf.Tags[0]);
        var edited = swf.InsertTag(1, clone);

        edited.Tags[1].ShouldBeSameAs(clone);
        edited.Tags.Count.ShouldBe(swf.Tags.Count + 1);
    }

    [Fact]
    public void RemoveTag_removes_the_referenced_instance()
    {
        var swf = Movie();
        var target = swf.Tags[0];
        var edited = swf.RemoveTag(target);

        edited.Tags.Count.ShouldBe(swf.Tags.Count - 1);
        edited.Tags.ShouldNotContain(target);
    }

    [Fact]
    public void MoveTag_reorders_the_referenced_instance()
    {
        var swf = Movie();
        var first = swf.Tags[0];
        var edited = swf.MoveTag(0, swf.Tags.Count - 1);

        edited.Tags[^1].ShouldBeSameAs(first);
    }

    [Fact]
    public void ReplaceTag_targets_the_referenced_instance_not_a_structural_duplicate()
    {
        var swf = Movie();
        var original = (SetBackgroundColorTag)swf.Tags[0];
        var duplicate = new SetBackgroundColorTag(original.Metadata, original.BackgroundColor);
        var file = new ShockwaveFlashFile(swf.Header, [original, duplicate, .. swf.Tags.Skip(1)]);

        var replacement = new SetBackgroundColorTag(original.Metadata, new Color(1, 2, 3, 4));
        var edited = file.ReplaceTag(duplicate, replacement);

        edited.Tags[0].ShouldBeSameAs(original);
        edited.Tags[1].ShouldBeSameAs(replacement);
    }

    [Fact]
    public void ReplaceTag_throws_when_the_tag_is_not_in_the_file()
    {
        var swf = Movie();
        var foreign = CloneBackground(swf.Tags[0]);

        Should.Throw<ArgumentException>(() => swf.ReplaceTag(foreign, foreign));
    }
}
