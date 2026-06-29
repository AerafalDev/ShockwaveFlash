using ShockwaveFlash.Avm1.Types;
using ShockwaveFlash.Tests.Models;
using ShockwaveFlash.Tests.Serialization;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class Avm1ConvertTests
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
}
