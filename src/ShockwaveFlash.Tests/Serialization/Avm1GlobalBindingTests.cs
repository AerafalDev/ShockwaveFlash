using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;
using ShockwaveFlash.Tests.Models;
using ShockwaveFlash.Tests.Serialization;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class Avm1GlobalBindingTests
{
    private static readonly TestModelsContext Ctx = TestModelsContext.Default;

    private static Player NewPlayer()
    {
        return new Player(
            "Kerubim",
            42,
            Rarity.Common,
            new Weapon("Sword", 10),
            null,
            [],
            [],
            new Dictionary<string, double>());
    }

    [Fact]
    public void Write_then_read_round_trips_through_the_binding_path()
    {
        var globals = new Avm1Object();

        Ctx.Write(globals, NewPlayer());

        globals.Members.ShouldContainKey("player");
        Ctx.Read<Player>(globals)!.Name.ShouldBe("Kerubim");
    }

    [Fact]
    public void Read_returns_default_when_the_global_is_absent()
    {
        Ctx.Read<Player>(new Avm1Object()).ShouldBeNull();
    }

    [Fact]
    public void Reflection_global_binding_uses_the_type_attribute_path()
    {
        var globals = new Avm1Object();

        Avm1Serializer.WriteGlobal(globals, NewPlayer());

        globals.Members.ShouldContainKey("player");
        Avm1Serializer.ReadGlobal<Player>(globals)!.Name.ShouldBe("Kerubim");
        Avm1Serializer.ReadGlobal<Player>(new Avm1Object()).ShouldBeNull();
    }

    [Fact]
    public void Reflection_and_generated_binding_agree()
    {
        var reflected = new Avm1Object();
        Avm1Serializer.WriteGlobal(reflected, NewPlayer());

        var generated = new Avm1Object();
        Ctx.Write(generated, NewPlayer());

        Avm1Trees.DeepEquals(reflected, generated).ShouldBeTrue();
    }
}
