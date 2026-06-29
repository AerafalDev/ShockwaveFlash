using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

public sealed record Inner(int V);

public sealed record Outer(
    [property: Avm1Property("c")] Inner Child,
    [property: Avm1Property("k")] List<Inner> Kids);

public sealed record NullableBag(Tier? E, Coord? Point, Tier F);

public sealed record Floats([property: Avm1Property("d")] double D);

[Avm1Serializable(typeof(Inner))]
[Avm1Serializable(typeof(Outer))]
[Avm1Serializable(typeof(NullableBag))]
[Avm1Serializable(typeof(Floats))]
public partial class GraphContext : Avm1SerializerContext;

public sealed class Avm1GraphTests
{
    [Fact]
    public void Registered_nested_child_resolves_through_the_context()
    {
        GraphContext.Default.GetTypeInfo(typeof(Inner)).ShouldNotBeNull();

        var value = new Outer(new Inner(1), [new Inner(2), new Inner(3)]);
        var obj = Avm1Serializer.Serialize(value, GraphContext.Default.Outer).AsObject;

        obj["c"].AsObject["V"].AsNumber.ShouldBe(1d);
        obj["k"].AsArray.Items[1].AsObject["V"].AsNumber.ShouldBe(3d);

        var back = Avm1Serializer.Deserialize(obj, GraphContext.Default.Outer);
        back.Child.V.ShouldBe(1);
        back.Kids[1].V.ShouldBe(3);

        Avm1Trees.DeepEquals(Avm1Serializer.Serialize(value), obj).ShouldBeTrue();
    }

    [Fact]
    public void Nullable_enum_and_nested_omit_and_restore_in_both_modes()
    {
        var absent = Avm1Serializer.Serialize(new NullableBag(null, null, Tier.Pro), GraphContext.Default.NullableBag).AsObject;
        absent.Members.ShouldNotContainKey("E");
        absent.Members.ShouldNotContainKey("Point");
        absent["F"].AsNumber.ShouldBe((double)(int)Tier.Pro);

        var value = new NullableBag(Tier.Max, new Coord(1, 2), Tier.Free);
        var present = Avm1Serializer.Serialize(value, GraphContext.Default.NullableBag).AsObject;
        present["E"].AsNumber.ShouldBe((double)(int)Tier.Max);

        var back = Avm1Serializer.Deserialize(present, GraphContext.Default.NullableBag);
        back.E.ShouldBe(Tier.Max);
        back.Point.ShouldBe(new Coord(1, 2));
        back.F.ShouldBe(Tier.Free);

        Avm1Trees.DeepEquals(Avm1Serializer.Serialize(value), present).ShouldBeTrue();
    }

    [Fact]
    public void SourceGen_reads_a_named_float_literal_from_a_string()
    {
        var ctx = new GraphContext(new Avm1SerializerOptions
        {
            NumberHandling = Avm1NumberHandling.AllowReadingFromString | Avm1NumberHandling.AllowNamedFloatingPointLiterals,
        });

        var obj = new Avm1Object { Members = { ["d"] = new Avm1String("Infinity") } };
        double.IsPositiveInfinity(Avm1Serializer.Deserialize(obj, ctx.Floats).D).ShouldBeTrue();
    }
}
