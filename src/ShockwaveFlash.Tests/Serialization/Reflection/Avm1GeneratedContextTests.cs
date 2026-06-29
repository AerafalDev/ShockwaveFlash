using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

[Avm1Serializable(typeof(Coord), "pos")]
[Avm1Serializable(typeof(Settings), "cfg")]
public partial class FixtureContext : Avm1SerializerContext;

public sealed class Avm1GeneratedContextTests
{
    [Fact]
    public void Generated_context_matches_reflection_for_constructor_type()
    {
        var value = new Coord(3, 4);

        var generated = Avm1Serializer.Serialize(value, FixtureContext.Default.Coord);

        Avm1Trees.DeepEquals(Avm1Serializer.Serialize(value), generated).ShouldBeTrue();
        Avm1Serializer.Deserialize(generated, FixtureContext.Default.Coord).ShouldBe(value);
    }

    [Fact]
    public void Generated_context_handles_init_only_object_initializer_type()
    {
        var value = new Settings { Theme = "dark", Mute = true };

        var generated = Avm1Serializer.Serialize(value, FixtureContext.Default.Settings).AsObject;
        generated["Theme"].AsString.ShouldBe("dark");
        generated["Mute"].AsBoolean.ShouldBeTrue();

        var back = Avm1Serializer.Deserialize(generated, FixtureContext.Default.Settings);
        back.Theme.ShouldBe("dark");
        back.Mute.ShouldBeTrue();
    }

    [Fact]
    public void Generated_context_path_binding_round_trips()
    {
        var globals = new Avm1Object();
        FixtureContext.Default.Write(globals, new Coord(1, 2));

        globals.Members["pos"].AsObject["X"].AsNumber.ShouldBe(1d);
        FixtureContext.Default.Read<Coord>(globals).ShouldBe(new Coord(1, 2));
    }

    [Fact]
    public void Generated_context_dispatch_resolves_only_registered_types()
    {
        FixtureContext.Default.ShouldBeSameAs(FixtureContext.Default);
        FixtureContext.Default.GetTypeInfo(typeof(Coord)).ShouldNotBeNull();
        FixtureContext.Default.GetTypeInfo(typeof(Settings)).ShouldNotBeNull();
        FixtureContext.Default.GetTypeInfo(typeof(int)).ShouldBeNull();
    }
}
