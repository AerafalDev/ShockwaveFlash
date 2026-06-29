using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Tests.Models;
using ShockwaveFlash.Tests.Serialization;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class GeneratedMapperTests
{
    private static readonly TestModelsContext Ctx = TestModelsContext.Default;

    private static Player NewPlayer()
    {
        return new Player(
            "Kerubim",
            42,
            Rarity.Epic,
            new Weapon("Sword", 10),
            new Weapon("Dagger", 3),
            [1, 2, 3],
            ["a", "b"],
            new Dictionary<string, double> { ["str"] = 1.5, ["agi"] = 2.0 });
    }

    [Fact]
    public void Serialize_writes_the_expected_tree()
    {
        var obj = Avm1Serializer.Serialize(NewPlayer(), Ctx.Player).AsObject;

        obj["name"].AsString.ShouldBe("Kerubim");
        obj["score"].AsNumber.ShouldBe(42d);
        obj["Rank"].AsNumber.ShouldBe((double)(int)Rarity.Epic);
        obj["Equipped"].AsObject["n"].AsString.ShouldBe("Sword");
        obj["Equipped"].AsObject["dmg"].AsNumber.ShouldBe(10d);
        obj["Sidearm"].AsObject["n"].AsString.ShouldBe("Dagger");
        obj["Inventory"].AsArray.Items.Count.ShouldBe(3);
        obj["Inventory"].AsArray.Items[0].AsNumber.ShouldBe(1d);
        obj["Tags"].AsArray.Items[1].AsString.ShouldBe("b");
        obj["Stats"].AsObject["str"].AsNumber.ShouldBe(1.5);
    }

    [Fact]
    public void Deserialize_reconstructs_all_members()
    {
        var back = Avm1Serializer.Deserialize(Avm1Serializer.Serialize(NewPlayer(), Ctx.Player), Ctx.Player);

        back.Name.ShouldBe("Kerubim");
        back.Score.ShouldBe(42);
        back.Rank.ShouldBe(Rarity.Epic);
        back.Equipped.Name.ShouldBe("Sword");
        back.Sidearm.ShouldNotBeNull();
        back.Sidearm!.Damage.ShouldBe(3);
        back.Inventory.ShouldBe(new[] { 1, 2, 3 });
        back.Tags.ShouldBe(new[] { "a", "b" });
        back.Stats["agi"].ShouldBe(2.0);
    }

    [Fact]
    public void Round_trip_is_stable()
    {
        var first = Avm1Serializer.Serialize(NewPlayer(), Ctx.Player);
        var second = Avm1Serializer.Serialize(Avm1Serializer.Deserialize(first, Ctx.Player), Ctx.Player);

        Avm1Trees.DeepEquals(first, second).ShouldBeTrue();
    }

    [Fact]
    public void Null_nullable_member_is_omitted()
    {
        var player = NewPlayer() with { Sidearm = null };
        var obj = Avm1Serializer.Serialize(player, Ctx.Player).AsObject;

        obj.Members.ShouldNotContainKey("Sidearm");
        Avm1Serializer.Deserialize(obj, Ctx.Player).Sidearm.ShouldBeNull();
    }

    [Fact]
    public void Binding_path_comes_from_the_attribute()
    {
        Ctx.Player.BindingPath.ShouldBe(["player"]);
        (Ctx.Weapon.BindingPath ?? []).ShouldBeEmpty();
    }
}
