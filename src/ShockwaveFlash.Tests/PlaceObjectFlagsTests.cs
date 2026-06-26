using ShockwaveFlash;
using ShockwaveFlash.Tags;
using ShockwaveFlash.Tags.Control;
using ShockwaveFlash.Tags.DisplayList;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.DisplayList;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class PlaceObjectFlagsTests
{
    private static ShockwaveFlashHeader Header()
    {
        return new ShockwaveFlashHeader(ShockwaveFlashCompression.None, 6, 0, new Rectangle(0, 0, 0, 0), new Fixed8(0), 1);
    }

    private static PlaceObject2Tag Place(string? name = null)
    {
        return new PlaceObject2Tag(new TagMetadata(TagCode.PlaceObject2, 0, 0), PlaceObjectAction.Place(1), 1, null, null, null, name, null, null);
    }

    private static PlaceObject2Tag RoundTrip(PlaceObject2Tag place)
    {
        var file = new ShockwaveFlashFile(Header(), [place, new EndTag(new TagMetadata(TagCode.End, 0, 0))]);
        var reparsed = ShockwaveFlashFile.Disassemble(file.Assemble());
        return reparsed.Tags.OfType<PlaceObject2Tag>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Adding_a_field_is_reflected_in_the_derived_flags_and_round_trips()
    {
        var place = Place();
        place.HasName.ShouldBeFalse();
        place.Flags.HasFlag(PlaceObjectFlags.HasName).ShouldBeFalse();

        place.Name = "hero";

        place.HasName.ShouldBeTrue();
        place.Flags.HasFlag(PlaceObjectFlags.HasName).ShouldBeTrue();

        var roundTripped = RoundTrip(place);
        roundTripped.Name.ShouldBe("hero");
        roundTripped.HasName.ShouldBeTrue();
    }

    [Fact]
    public void Removing_a_field_clears_the_derived_flag_and_round_trips()
    {
        var place = Place("hero");
        place.HasName.ShouldBeTrue();

        place.Name = null;

        place.HasName.ShouldBeFalse();
        place.Flags.HasFlag(PlaceObjectFlags.HasName).ShouldBeFalse();

        var roundTripped = RoundTrip(place);
        roundTripped.Name.ShouldBeNull();
        roundTripped.HasName.ShouldBeFalse();
    }
}
