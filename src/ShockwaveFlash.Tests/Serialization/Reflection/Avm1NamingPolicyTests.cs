using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

public sealed record Named(int FooBar, [property: Avm1Property("explicit_key")] string BazQux);

[Avm1Serializable(typeof(Named))]
public partial class NamedContext : Avm1SerializerContext;

public sealed class Avm1NamingPolicyTests
{
    [Fact]
    public void Camel_case_policy_renames_unkeyed_members_in_both_modes()
    {
        var value = new Named(1, "z");

        var reflected = Avm1Serializer.Serialize(value, new Avm1SerializerOptions { PropertyNamingPolicy = Avm1NamingPolicy.CamelCase }).AsObject;
        reflected.Members.ShouldContainKey("fooBar");
        reflected.Members.ShouldContainKey("explicit_key");
        reflected.Members.ShouldNotContainKey("FooBar");

        var ctx = new NamedContext(new Avm1SerializerOptions { PropertyNamingPolicy = Avm1NamingPolicy.CamelCase });
        var generated = Avm1Serializer.Serialize(value, ctx.Named).AsObject;
        generated.Members.ShouldContainKey("fooBar");
        generated.Members.ShouldContainKey("explicit_key");

        Avm1Trees.DeepEquals(reflected, generated).ShouldBeTrue();

        var back = Avm1Serializer.Deserialize(generated, ctx.Named);
        back.FooBar.ShouldBe(1);
        back.BazQux.ShouldBe("z");
    }

    [Fact]
    public void No_policy_keeps_member_names()
    {
        var obj = Avm1Serializer.Serialize(new Named(1, "z"), NamedContext.Default.Named).AsObject;

        obj.Members.ShouldContainKey("FooBar");
        obj.Members.ShouldContainKey("explicit_key");
    }
}
