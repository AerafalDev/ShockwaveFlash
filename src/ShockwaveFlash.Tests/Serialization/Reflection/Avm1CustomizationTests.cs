using System.Linq;
using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

public sealed record RequiredCount([property: Avm1Required] int Count);

public sealed class MultiCtor
{
    public MultiCtor(int a)
    {
        A = a;
    }

    [Avm1Constructor]
    public MultiCtor(int a, int b)
    {
        A = a;
        B = b;
    }

    public int A { get; }

    public int B { get; }
}

public sealed record Ordered(
    [property: Avm1PropertyOrder(2)] int Second,
    [property: Avm1PropertyOrder(1)] int First);

[Avm1Serializable(typeof(RequiredCount))]
[Avm1Serializable(typeof(MultiCtor))]
[Avm1Serializable(typeof(Ordered))]
public partial class CustomizationContext : Avm1SerializerContext;

[Avm1Serializable(typeof(Coord), Segments = new[] { "a.b", "c" })]
[Avm1Serializable(typeof(Coord), "p1", TypeInfoPropertyName = "First")]
[Avm1Serializable(typeof(Coord), "p2", TypeInfoPropertyName = "Second")]
public partial class PathContext : Avm1SerializerContext;

public sealed class Avm1CustomizationTests
{
    [Fact]
    public void Required_forces_a_value_scalar_to_throw_when_missing()
    {
        Should.Throw<Avm1SerializationException>(() => Avm1Serializer.Deserialize<RequiredCount>(new Avm1Object()));
        Should.Throw<Avm1SerializationException>(() => Avm1Serializer.Deserialize(new Avm1Object(), CustomizationContext.Default.RequiredCount));

        var present = new Avm1Object { Members = { ["Count"] = new Avm1Number(5) } };
        Avm1Serializer.Deserialize<RequiredCount>(present).Count.ShouldBe(5);
        Avm1Serializer.Deserialize(present, CustomizationContext.Default.RequiredCount).Count.ShouldBe(5);
    }

    [Fact]
    public void Constructor_attribute_selects_the_constructor()
    {
        var obj = new Avm1Object { Members = { ["A"] = new Avm1Number(1), ["B"] = new Avm1Number(2) } };

        var reflected = Avm1Serializer.Deserialize<MultiCtor>(obj);
        reflected.A.ShouldBe(1);
        reflected.B.ShouldBe(2);

        var generated = Avm1Serializer.Deserialize(obj, CustomizationContext.Default.MultiCtor);
        generated.A.ShouldBe(1);
        generated.B.ShouldBe(2);
    }

    [Fact]
    public void Property_order_controls_write_order_in_both_modes()
    {
        var value = new Ordered(20, 10);

        foreach (var obj in new[] { Avm1Serializer.Serialize(value).AsObject, Avm1Serializer.Serialize(value, CustomizationContext.Default.Ordered).AsObject })
        {
            var keys = obj.Members.Keys.ToList();
            keys.IndexOf("First").ShouldBeLessThan(keys.IndexOf("Second"));
        }
    }

    [Fact]
    public void Segments_escape_a_dotted_key()
    {
        PathContext.Default.Coord.BindingPath.ShouldBe(["a.b", "c"]);

        var globals = new Avm1Object();
        PathContext.Default.Write(globals, new Coord(1, 2));

        globals.Members["a.b"].AsObject["c"].AsObject["X"].AsNumber.ShouldBe(1d);
        PathContext.Default.Read<Coord>(globals).ShouldBe(new Coord(1, 2));
    }

    [Fact]
    public void TypeInfoPropertyName_registers_a_type_at_multiple_paths()
    {
        PathContext.Default.First.BindingPath.ShouldBe(["p1"]);
        PathContext.Default.Second.BindingPath.ShouldBe(["p2"]);

        var globals = new Avm1Object();
        Avm1Serializer.WriteGlobal(globals, new Coord(1, 2), PathContext.Default.First);
        Avm1Serializer.WriteGlobal(globals, new Coord(3, 4), PathContext.Default.Second);

        globals.Members["p1"].AsObject["X"].AsNumber.ShouldBe(1d);
        globals.Members["p2"].AsObject["X"].AsNumber.ShouldBe(3d);
        Avm1Serializer.ReadGlobal(globals, PathContext.Default.First).ShouldBe(new Coord(1, 2));
    }
}
