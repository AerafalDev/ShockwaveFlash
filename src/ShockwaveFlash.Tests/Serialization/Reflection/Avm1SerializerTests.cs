using System.Collections.Generic;
using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

public sealed class Avm1SerializerTests
{
    [Fact]
    public void Numbers_round_trip_every_numeric_type()
    {
        var value = new Numbers(1, -2, 3, 4, -5, 6, -7, 8, 1.5f, 2.5, 3.25m, true, Tier.Max);
        var obj = Avm1Serializer.Serialize(value).AsObject;

        obj["I"].AsNumber.ShouldBe(-5d);
        obj["M"].AsNumber.ShouldBe(3.25d);
        obj["Bo"].AsBoolean.ShouldBeTrue();
        obj["E"].AsNumber.ShouldBe((double)(int)Tier.Max);

        Avm1Serializer.Deserialize<Numbers>(obj).ShouldBe(value);
    }

    [Fact]
    public void Top_level_scalars_round_trip()
    {
        Avm1Serializer.Deserialize<int>(new Avm1Number(42)).ShouldBe(42);
        Avm1Serializer.Deserialize<string>(new Avm1String("hi")).ShouldBe("hi");
        Avm1Serializer.Deserialize<bool>(new Avm1Boolean(true)).ShouldBeTrue();
        Avm1Serializer.Deserialize<Tier>(new Avm1Number(1)).ShouldBe(Tier.Pro);
        Avm1Serializer.Serialize(42).ShouldBeOfType<Avm1Number>().Value.ShouldBe(42d);
    }

    [Fact]
    public void Collections_and_jagged_round_trip()
    {
        var value = new Nested("k", new Coord(1, 2), [1, 2, 3], [[1, 2], [3]], new Dictionary<string, int> { ["a"] = 1 });
        var obj = Avm1Serializer.Serialize(value).AsObject;

        obj["Values"].AsArray.Items.Count.ShouldBe(3);
        obj["Grid"].AsArray.Items[0].AsArray.Items[1].AsNumber.ShouldBe(2d);
        obj["Map"].AsObject["a"].AsNumber.ShouldBe(1d);
        obj["Point"].AsObject["X"].AsNumber.ShouldBe(1d);

        var back = Avm1Serializer.Deserialize<Nested>(obj);
        back.Values.ShouldBe(new[] { 1, 2, 3 });
        back.Grid[0].ShouldBe(new[] { 1, 2 });
        back.Map["a"].ShouldBe(1);
    }

    [Fact]
    public void Top_level_dictionary_round_trips()
    {
        var dict = new Dictionary<string, int> { ["x"] = 1, ["y"] = 2 };
        var obj = Avm1Serializer.Serialize(dict).AsObject;

        obj["x"].AsNumber.ShouldBe(1d);
        Avm1Serializer.Deserialize<Dictionary<string, int>>(obj)["y"].ShouldBe(2);
    }

    [Fact]
    public void Passthrough_is_identity()
    {
        var array = new Avm1Array();
        array.Items.Add(new Avm1String("x"));

        Avm1Serializer.Serialize(array).ShouldBeSameAs(array);
        Avm1Serializer.Deserialize<Avm1Array>(array).ShouldBeSameAs(array);
        Avm1Serializer.Deserialize<Avm1Value>(new Avm1Number(3)).ShouldBeOfType<Avm1Number>();
    }

    [Fact]
    public void Nullable_members_omit_null_and_read_back_null()
    {
        Avm1Serializer.Serialize(new Optionals(null, null, null, null)).AsObject.Members.ShouldBeEmpty();

        var back = Avm1Serializer.Deserialize<Optionals>(new Avm1Object());
        back.A.ShouldBeNull();
        back.Name.ShouldBeNull();
        back.Nums.ShouldBeNull();
        back.Point.ShouldBeNull();

        var present = Avm1Serializer.Serialize(new Optionals(5, "n", [1], new Coord(1, 2))).AsObject;
        present["A"].AsNumber.ShouldBe(5d);
        Avm1Serializer.Deserialize<Optionals>(present).Point.ShouldBe(new Coord(1, 2));
    }

    [Fact]
    public void Property_name_and_ignore_are_honored()
    {
        var obj = Avm1Serializer.Serialize(new Keyed { Name = "bob", Skip = 9, Keep = 7 }).AsObject;

        obj["n"].AsString.ShouldBe("bob");
        obj["Keep"].AsNumber.ShouldBe(7d);
        obj.Members.ShouldNotContainKey("Skip");
        obj.Members.ShouldNotContainKey("Name");

        var back = Avm1Serializer.Deserialize<Keyed>(obj);
        back.Name.ShouldBe("bob");
        back.Keep.ShouldBe(7);
        back.Skip.ShouldBe(0);
    }

    [Fact]
    public void Mutable_type_uses_object_initializer()
    {
        var obj = Avm1Serializer.Serialize(new Settings { Theme = "dark", Mute = true }).AsObject;

        obj["Theme"].AsString.ShouldBe("dark");

        var back = Avm1Serializer.Deserialize<Settings>(obj);
        back.Theme.ShouldBe("dark");
        back.Mute.ShouldBeTrue();
    }

    [Fact]
    public void Member_converter_attribute_is_used()
    {
        var obj = Avm1Serializer.Serialize(new WithCoordAttr(new Coord(3, 4))).AsObject;

        obj["Position"].AsString.ShouldBe("3,4");
        Avm1Serializer.Deserialize<WithCoordAttr>(obj).Position.ShouldBe(new Coord(3, 4));
    }

    [Fact]
    public void Type_converter_attribute_is_used()
    {
        var obj = Avm1Serializer.Serialize(new WithVector(new Vector(5, 6))).AsObject;

        obj["V"].AsString.ShouldBe("5,6");
        Avm1Serializer.Deserialize<WithVector>(obj).V.ShouldBe(new Vector(5, 6));
    }

    [Fact]
    public void Options_converter_is_used()
    {
        var options = new Avm1SerializerOptions();
        options.Converters.Add(new CoordConverter());

        Avm1Serializer.Serialize(new Coord(1, 2), options).ShouldBeOfType<Avm1String>().Value.ShouldBe("1,2");
        Avm1Serializer.Deserialize<Coord>(new Avm1String("7,8"), options).ShouldBe(new Coord(7, 8));
    }

    [Fact]
    public void Converter_factory_is_used()
    {
        var options = new Avm1SerializerOptions();
        options.Converters.Add(new CoordFactory());

        Avm1Serializer.Serialize(new Coord(8, 9), options).ShouldBeOfType<Avm1String>().Value.ShouldBe("8,9");
    }

    [Fact]
    public void Property_converter_wins_over_options_and_type()
    {
        var options = new Avm1SerializerOptions();
        options.Converters.Add(new TaggedOptionsConverter());

        Avm1Serializer.Serialize(new Holder(new Tagged(7)), options).AsObject["T"].AsString.ShouldBe("prop:7");
    }

    [Fact]
    public void Options_converter_wins_over_type_attribute()
    {
        var options = new Avm1SerializerOptions();
        options.Converters.Add(new TaggedOptionsConverter());

        Avm1Serializer.Serialize(new Holder2(new Tagged(7)), options).AsObject["T"].AsString.ShouldBe("opt:7");
    }

    [Fact]
    public void Type_attribute_wins_over_built_in()
    {
        Avm1Serializer.Serialize(new Holder2(new Tagged(7))).AsObject["T"].AsString.ShouldBe("type:7");
    }

    [Fact]
    public void NumberHandling_reads_numbers_from_strings()
    {
        var options = new Avm1SerializerOptions { NumberHandling = Avm1NumberHandling.AllowReadingFromString };

        Avm1Serializer.Deserialize<int>(new Avm1String("42"), options).ShouldBe(42);
    }

    [Fact]
    public void NumberHandling_reads_named_floating_point_literals()
    {
        var options = new Avm1SerializerOptions
        {
            NumberHandling = Avm1NumberHandling.AllowReadingFromString | Avm1NumberHandling.AllowNamedFloatingPointLiterals,
        };

        double.IsNaN(Avm1Serializer.Deserialize<double>(new Avm1String("NaN"), options)).ShouldBeTrue();
        double.IsPositiveInfinity(Avm1Serializer.Deserialize<double>(new Avm1String("Infinity"), options)).ShouldBeTrue();
    }

    [Fact]
    public void NumberHandling_writes_numbers_as_strings()
    {
        var options = new Avm1SerializerOptions { NumberHandling = Avm1NumberHandling.WriteAsString };

        Avm1Serializer.Serialize(42, options).ShouldBeOfType<Avm1String>().Value.ShouldBe("42");
    }

    [Fact]
    public void IgnoreCondition_never_writes_null()
    {
        var options = new Avm1SerializerOptions { DefaultIgnoreCondition = Avm1IgnoreCondition.Never };
        var obj = Avm1Serializer.Serialize(new Optionals(null, null, null, null), options).AsObject;

        obj["A"].IsNull.ShouldBeTrue();
        obj["Name"].IsNull.ShouldBeTrue();
    }

    [Fact]
    public void IgnoreCondition_always_skips_everything()
    {
        var options = new Avm1SerializerOptions { DefaultIgnoreCondition = Avm1IgnoreCondition.Always };

        Avm1Serializer.Serialize(new Default(5), options).AsObject.Members.ShouldBeEmpty();
    }

    [Fact]
    public void IgnoreCondition_when_writing_default_skips_defaults()
    {
        var options = new Avm1SerializerOptions { DefaultIgnoreCondition = Avm1IgnoreCondition.WhenWritingDefault };

        Avm1Serializer.Serialize(new Default(0), options).AsObject.Members.ShouldNotContainKey("A");
        Avm1Serializer.Serialize(new Default(5), options).AsObject["A"].AsNumber.ShouldBe(5d);
    }

    [Fact]
    public void Missing_required_member_throws()
    {
        Should.Throw<Avm1SerializationException>(() => Avm1Serializer.Deserialize<Nested>(new Avm1Object()));
    }

    [Fact]
    public void Complex_object_round_trip_is_stable()
    {
        var value = new Nested("k", new Coord(1, 2), [1, 2, 3], [[1], [2, 3]], new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 });

        var first = Avm1Serializer.Serialize(value);
        var second = Avm1Serializer.Serialize(Avm1Serializer.Deserialize<Nested>(first));

        Avm1Trees.DeepEquals(first, second).ShouldBeTrue();
    }
}
