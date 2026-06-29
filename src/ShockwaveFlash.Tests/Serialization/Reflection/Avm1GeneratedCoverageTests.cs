using System.Collections.Generic;
using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

[Avm1Serializable(typeof(Numbers))]
[Avm1Serializable(typeof(Optionals))]
[Avm1Serializable(typeof(Keyed))]
[Avm1Serializable(typeof(Settings))]
[Avm1Serializable(typeof(Nested))]
[Avm1Serializable(typeof(WithCoordAttr))]
[Avm1Serializable(typeof(WithVector))]
[Avm1Serializable(typeof(Holder))]
public partial class CoverageContext : Avm1SerializerContext;

public sealed class Avm1GeneratedCoverageTests
{
    private static readonly CoverageContext Ctx = CoverageContext.Default;

    [Fact]
    public void Every_numeric_type_widens_like_reflection()
    {
        var value = new Numbers(1, -2, 3, 4, -5, 6, -7, 8, 1.5f, 2.5, 3.25m, true, Tier.Max);
        var obj = Avm1Serializer.Serialize(value, Ctx.Numbers).AsObject;

        obj["I"].AsNumber.ShouldBe(-5d);
        obj["M"].AsNumber.ShouldBe(3.25d);
        obj["Bo"].AsBoolean.ShouldBeTrue();
        obj["E"].AsNumber.ShouldBe((double)(int)Tier.Max);

        Avm1Serializer.Deserialize(obj, Ctx.Numbers).ShouldBe(value);
        Avm1Trees.DeepEquals(Avm1Serializer.Serialize(value), obj).ShouldBeTrue();
    }

    [Fact]
    public void Nullable_members_omit_and_read_back_null()
    {
        Avm1Serializer.Serialize(new Optionals(null, null, null, null), Ctx.Optionals).AsObject.Members.ShouldBeEmpty();

        var back = Avm1Serializer.Deserialize(new Avm1Object(), Ctx.Optionals);
        back.A.ShouldBeNull();
        back.Name.ShouldBeNull();
        back.Nums.ShouldBeNull();
        back.Point.ShouldBeNull();

        var present = Avm1Serializer.Serialize(new Optionals(5, "n", [1], new Coord(1, 2)), Ctx.Optionals).AsObject;
        present["A"].AsNumber.ShouldBe(5d);
        Avm1Serializer.Deserialize(present, Ctx.Optionals).Point.ShouldBe(new Coord(1, 2));
    }

    [Fact]
    public void Nested_collections_and_jagged_arrays_round_trip()
    {
        var value = new Nested("k", new Coord(1, 2), [1, 2, 3], [[1, 2], [3]], new Dictionary<string, int> { ["a"] = 1 });
        var obj = Avm1Serializer.Serialize(value, Ctx.Nested).AsObject;

        obj["Values"].AsArray.Items.Count.ShouldBe(3);
        obj["Grid"].AsArray.Items[0].AsArray.Items[1].AsNumber.ShouldBe(2d);
        obj["Map"].AsObject["a"].AsNumber.ShouldBe(1d);
        obj["Point"].AsObject["X"].AsNumber.ShouldBe(1d);

        var back = Avm1Serializer.Deserialize(obj, Ctx.Nested);
        back.Values.ShouldBe(new[] { 1, 2, 3 });
        back.Grid[0].ShouldBe(new[] { 1, 2 });
        back.Map["a"].ShouldBe(1);
        Avm1Trees.DeepEquals(Avm1Serializer.Serialize(value), obj).ShouldBeTrue();
    }

    [Fact]
    public void Property_name_and_ignore_are_honored()
    {
        var obj = Avm1Serializer.Serialize(new Keyed { Name = "bob", Skip = 9, Keep = 7 }, Ctx.Keyed).AsObject;

        obj["n"].AsString.ShouldBe("bob");
        obj["Keep"].AsNumber.ShouldBe(7d);
        obj.Members.ShouldNotContainKey("Skip");

        var back = Avm1Serializer.Deserialize(obj, Ctx.Keyed);
        back.Name.ShouldBe("bob");
        back.Keep.ShouldBe(7);
        back.Skip.ShouldBe(0);
    }

    [Fact]
    public void Object_initializer_type_with_init_members_round_trips()
    {
        var obj = Avm1Serializer.Serialize(new Settings { Theme = "dark", Mute = true }, Ctx.Settings);
        var back = Avm1Serializer.Deserialize(obj, Ctx.Settings);

        back.Theme.ShouldBe("dark");
        back.Mute.ShouldBeTrue();
    }

    [Fact]
    public void Member_converter_attribute_is_used()
    {
        var obj = Avm1Serializer.Serialize(new WithCoordAttr(new Coord(3, 4)), Ctx.WithCoordAttr).AsObject;

        obj["Position"].AsString.ShouldBe("3,4");
        Avm1Serializer.Deserialize(obj, Ctx.WithCoordAttr).Position.ShouldBe(new Coord(3, 4));
    }

    [Fact]
    public void Converter_on_the_member_type_is_used()
    {
        var obj = Avm1Serializer.Serialize(new WithVector(new Vector(5, 6)), Ctx.WithVector).AsObject;

        obj["V"].AsString.ShouldBe("5,6");
        Avm1Serializer.Deserialize(obj, Ctx.WithVector).V.ShouldBe(new Vector(5, 6));
    }

    [Fact]
    public void Property_converter_wins_over_the_type_converter()
    {
        var obj = Avm1Serializer.Serialize(new Holder(new Tagged(7)), Ctx.Holder).AsObject;

        obj["T"].AsString.ShouldBe("prop:7");
    }

    [Fact]
    public void Missing_required_member_throws()
    {
        Should.Throw<Avm1SerializationException>(() => Avm1Serializer.Deserialize(new Avm1Object(), Ctx.Nested));
    }

    [Fact]
    public void Generated_trees_match_reflection_across_fixtures()
    {
        Match(new Numbers(9, -8, 7, 6, -5, 4, -3, 2, 0.5f, 0.25, 0.125m, false, Tier.Pro), Ctx.Numbers);
        Match(new Optionals(3, "x", [9, 8], new Coord(4, 5)), Ctx.Optionals);
        Match(new Keyed { Name = "z", Keep = 1 }, Ctx.Keyed);
        Match(new Settings { Theme = null, Mute = true }, Ctx.Settings);
        Match(new Nested("n", new Coord(6, 7), [4], [[8], [9, 0]], new Dictionary<string, int> { ["b"] = 2 }), Ctx.Nested);
        Match(new WithCoordAttr(new Coord(1, 1)), Ctx.WithCoordAttr);
        Match(new WithVector(new Vector(2, 2)), Ctx.WithVector);
        Match(new Holder(new Tagged(3)), Ctx.Holder);
    }

    private static void Match<T>(T value, Avm1TypeInfo<T> typeInfo)
    {
        var reflected = Avm1Serializer.Serialize(value);
        var generated = Avm1Serializer.Serialize(value, typeInfo);

        Avm1Trees.DeepEquals(reflected, generated).ShouldBeTrue($"{typeof(T).Name} generated tree differs from reflection");
    }
}
