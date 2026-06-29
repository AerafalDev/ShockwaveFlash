using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;
using ShockwaveFlash.Tests.Models;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class Avm1ConvertTests
{
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
    public void WriteGlobal_then_ReadGlobal_round_trips()
    {
        var globals = new Avm1Object();

        Avm1Convert.WriteGlobal(globals, NewPlayer());

        globals.Members.ShouldContainKey("player");
        Avm1Convert.ReadGlobal<Player>(globals).Name.ShouldBe("Kerubim");
    }

    [Fact]
    public void TryReadGlobal_returns_false_when_absent()
    {
        Avm1Convert.TryReadGlobal<Player>(new Avm1Object(), out var player).ShouldBeFalse();
        player.ShouldBeNull();
    }

    [Fact]
    public void ReadGlobal_throws_when_global_is_missing()
    {
        Should.Throw<Avm1SerializationException>(() => Avm1Convert.ReadGlobal<Player>(new Avm1Object()));
    }
}
