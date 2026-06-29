using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

public sealed record WithComputed(int A)
{
    [Avm1Include]
    public int Doubled => A * 2;
}

[Avm1Serializable(typeof(WithComputed))]
public partial class IncludeContext : Avm1SerializerContext;

public sealed class Avm1IncludeTests
{
    [Fact]
    public void Include_serializes_a_computed_get_only_member_in_both_modes()
    {
        var value = new WithComputed(5);

        var reflected = Avm1Serializer.Serialize(value).AsObject;
        reflected["Doubled"].AsNumber.ShouldBe(10d);

        var generated = Avm1Serializer.Serialize(value, IncludeContext.Default.WithComputed).AsObject;
        generated["Doubled"].AsNumber.ShouldBe(10d);

        Avm1Trees.DeepEquals(reflected, generated).ShouldBeTrue();
    }

    [Fact]
    public void Included_member_is_ignored_on_read()
    {
        var obj = new Avm1Object { Members = { ["A"] = new Avm1Number(5), ["Doubled"] = new Avm1Number(99) } };

        Avm1Serializer.Deserialize(obj, IncludeContext.Default.WithComputed).Doubled.ShouldBe(10);
        Avm1Serializer.Deserialize<WithComputed>(obj).Doubled.ShouldBe(10);
    }
}
