using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

public sealed record Scalar(int I);

public record struct Point3(int X, int Y);

public sealed class FieldBag
{
    public int Count;

    public string Label = "";
}

public sealed record Collections(List<int> Nums, int[]? Maybe, Dictionary<string, int> Map);

public sealed record HasDefault(int A, bool B);

[Avm1Serializable(typeof(Scalar))]
[Avm1Serializable(typeof(Point3))]
[Avm1Serializable(typeof(FieldBag))]
[Avm1Serializable(typeof(Collections))]
[Avm1Serializable(typeof(HasDefault))]
public partial class ThoroughContext : Avm1SerializerContext;

public sealed class Avm1ThoroughTests
{
    [Fact]
    public void SourceGen_number_handling_reads_from_string()
    {
        var ctx = new ThoroughContext(new Avm1SerializerOptions { NumberHandling = Avm1NumberHandling.AllowReadingFromString });
        var obj = new Avm1Object { Members = { ["I"] = new Avm1String("42") } };

        Avm1Serializer.Deserialize(obj, ctx.Scalar).I.ShouldBe(42);
    }

    [Fact]
    public void SourceGen_number_handling_writes_as_string()
    {
        var ctx = new ThoroughContext(new Avm1SerializerOptions { NumberHandling = Avm1NumberHandling.WriteAsString });

        Avm1Serializer.Serialize(new Scalar(7), ctx.Scalar).AsObject["I"].ShouldBeOfType<Avm1String>().Value.ShouldBe("7");
    }

    [Fact]
    public void SourceGen_ignore_condition_never_writes_null()
    {
        var ctx = new ThoroughContext(new Avm1SerializerOptions { DefaultIgnoreCondition = Avm1IgnoreCondition.Never });

        Avm1Serializer.Serialize(new Collections([], null, new Dictionary<string, int>()), ctx.Collections).AsObject["Maybe"].IsNull.ShouldBeTrue();
    }

    [Fact]
    public void SourceGen_ignore_condition_when_writing_default_skips_defaults()
    {
        var ctx = new ThoroughContext(new Avm1SerializerOptions { DefaultIgnoreCondition = Avm1IgnoreCondition.WhenWritingDefault });

        var obj = Avm1Serializer.Serialize(new HasDefault(0, false), ctx.HasDefault).AsObject;
        obj.Members.ShouldNotContainKey("A");
        obj.Members.ShouldNotContainKey("B");

        Avm1Serializer.Serialize(new HasDefault(5, true), ctx.HasDefault).AsObject["A"].AsNumber.ShouldBe(5d);
    }

    [Fact]
    public void Record_struct_round_trips_in_both_modes()
    {
        var value = new Point3(3, 4);

        var reflected = Avm1Serializer.Serialize(value);
        var generated = Avm1Serializer.Serialize(value, ThoroughContext.Default.Point3);

        Avm1Trees.DeepEquals(reflected, generated).ShouldBeTrue();
        Avm1Serializer.Deserialize(generated, ThoroughContext.Default.Point3).ShouldBe(value);
        Avm1Serializer.Deserialize<Point3>(reflected).ShouldBe(value);
    }

    [Fact]
    public void Public_fields_round_trip_in_both_modes()
    {
        var value = new FieldBag { Count = 9, Label = "hi" };

        var reflected = Avm1Serializer.Serialize(value).AsObject;
        var generated = Avm1Serializer.Serialize(value, ThoroughContext.Default.FieldBag).AsObject;

        reflected["Count"].AsNumber.ShouldBe(9d);
        generated["Label"].AsString.ShouldBe("hi");
        Avm1Trees.DeepEquals(reflected, generated).ShouldBeTrue();

        var back = Avm1Serializer.Deserialize(generated, ThoroughContext.Default.FieldBag);
        back.Count.ShouldBe(9);
        back.Label.ShouldBe("hi");
    }

    [Fact]
    public void Empty_and_null_collections_round_trip()
    {
        var value = new Collections([1, 2], null, new Dictionary<string, int> { ["k"] = 3 });
        var obj = Avm1Serializer.Serialize(value, ThoroughContext.Default.Collections).AsObject;

        obj["Nums"].AsArray.Items.Count.ShouldBe(2);
        obj.Members.ShouldNotContainKey("Maybe");

        var back = Avm1Serializer.Deserialize(obj, ThoroughContext.Default.Collections);
        back.Nums.ShouldBe(new[] { 1, 2 });
        back.Maybe.ShouldBeNull();
        back.Map["k"].ShouldBe(3);

        var empty = Avm1Serializer.Deserialize(new Avm1Object(), ThoroughContext.Default.Collections);
        empty.Nums.ShouldBeEmpty();
        empty.Map.ShouldBeEmpty();
        empty.Maybe.ShouldBeNull();
    }

    [Fact]
    public void Round_trip_is_stable_across_fixtures()
    {
        Stable(new Scalar(11), ThoroughContext.Default.Scalar);
        Stable(new Point3(1, 2), ThoroughContext.Default.Point3);
        Stable(new FieldBag { Count = 3, Label = "x" }, ThoroughContext.Default.FieldBag);
        Stable(new Collections([4, 5], [6], new Dictionary<string, int> { ["a"] = 7 }), ThoroughContext.Default.Collections);
        Stable(new HasDefault(8, true), ThoroughContext.Default.HasDefault);
    }

    private static void Stable<T>(T value, Avm1TypeInfo<T> typeInfo)
    {
        var first = Avm1Serializer.Serialize(value, typeInfo);
        var second = Avm1Serializer.Serialize(Avm1Serializer.Deserialize(first, typeInfo), typeInfo);

        Avm1Trees.DeepEquals(first, second).ShouldBeTrue($"{typeof(T).Name} is not round-trip stable");
    }
}
