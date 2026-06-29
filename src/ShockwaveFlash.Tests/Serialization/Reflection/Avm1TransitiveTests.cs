using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

public sealed record Leaf(int V);

public sealed record Root(
    [property: Avm1Property("c")] Leaf Child,
    [property: Avm1Property("ks")] List<Leaf> Kids);

[Avm1Serializable(typeof(Root))]
public partial class TransitiveContext : Avm1SerializerContext;

public sealed class Avm1TransitiveTests
{
    [Fact]
    public void Nested_types_are_registered_transitively()
    {
        TransitiveContext.Default.GetTypeInfo(typeof(Root)).ShouldNotBeNull();
        TransitiveContext.Default.GetTypeInfo(typeof(Leaf)).ShouldNotBeNull();

        var value = new Root(new Leaf(1), [new Leaf(2), new Leaf(3)]);
        var obj = Avm1Serializer.Serialize(value, TransitiveContext.Default.Root).AsObject;

        obj["c"].AsObject["V"].AsNumber.ShouldBe(1d);
        obj["ks"].AsArray.Items[1].AsObject["V"].AsNumber.ShouldBe(3d);

        var back = Avm1Serializer.Deserialize(obj, TransitiveContext.Default.Root);
        back.Child.V.ShouldBe(1);
        back.Kids[1].V.ShouldBe(3);
    }
}
