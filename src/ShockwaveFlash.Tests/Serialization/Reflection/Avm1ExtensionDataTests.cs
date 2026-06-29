using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

public sealed record WithExtra(
    [property: Avm1Property("n")] string Name,
    [property: Avm1ExtensionData] Dictionary<string, Avm1Value> Extra);

[Avm1Serializable(typeof(WithExtra))]
public partial class ExtraContext : Avm1SerializerContext;

public sealed class Avm1ExtensionDataTests
{
    private static Avm1Object Sample()
    {
        return new Avm1Object
        {
            Members =
            {
                ["n"] = new Avm1String("bob"),
                ["x"] = new Avm1Number(1),
                ["y"] = new Avm1String("z"),
            },
        };
    }

    [Fact]
    public void Reflection_captures_and_restores_unknown_members()
    {
        var value = Avm1Serializer.Deserialize<WithExtra>(Sample());

        value.Name.ShouldBe("bob");
        value.Extra.Count.ShouldBe(2);
        value.Extra["x"].AsNumber.ShouldBe(1d);
        value.Extra["y"].AsString.ShouldBe("z");

        var written = Avm1Serializer.Serialize(value).AsObject;
        written["n"].AsString.ShouldBe("bob");
        written["x"].AsNumber.ShouldBe(1d);
        written["y"].AsString.ShouldBe("z");
        written.Members.Count.ShouldBe(3);
    }

    [Fact]
    public void Generated_captures_and_restores_unknown_members()
    {
        var value = Avm1Serializer.Deserialize(Sample(), ExtraContext.Default.WithExtra);

        value.Name.ShouldBe("bob");
        value.Extra.Count.ShouldBe(2);
        value.Extra["x"].AsNumber.ShouldBe(1d);

        var written = Avm1Serializer.Serialize(value, ExtraContext.Default.WithExtra).AsObject;
        written["x"].AsNumber.ShouldBe(1d);
        written["y"].AsString.ShouldBe("z");
        written.Members.Count.ShouldBe(3);
    }

    [Fact]
    public void Reflection_and_generated_agree_on_the_round_trip()
    {
        var reflected = Avm1Serializer.Serialize(Avm1Serializer.Deserialize<WithExtra>(Sample())).AsObject;
        var generated = Avm1Serializer.Serialize(Avm1Serializer.Deserialize(Sample(), ExtraContext.Default.WithExtra), ExtraContext.Default.WithExtra).AsObject;

        Avm1Trees.DeepEquals(reflected, generated).ShouldBeTrue();
    }
}
