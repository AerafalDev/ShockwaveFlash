using ShockwaveFlash.Avm1.Types;
using ShockwaveFlash.Tests.Models;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class NestedContainerTests
{
    [Fact]
    public void Nested_containers_and_passthrough_round_trip()
    {
        var raw = new Avm1Array();
        raw.Items.Add(new Avm1Number(7));
        var inner = new Avm1Array();
        inner.Items.Add(new Avm1String("x"));
        raw.Items.Add(inner);

        var model = new Nested(
            new Dictionary<string, bool[]> { ["a"] = [true, false], ["b"] = [false] },
            [[1, 2], [3]],
            new Dictionary<string, Dictionary<string, int>> { ["g"] = new() { ["x"] = 1, ["y"] = 2 } },
            raw,
            null);

        var obj = model.ToAvm1Object();

        obj.Members.ShouldNotContainKey("opt");

        var back = Nested.FromAvm1Object(obj);

        Avm1Trees.DeepEquals(obj, back.ToAvm1Object()).ShouldBeTrue();
        back.Flags["a"].ShouldBe(new[] { true, false });
        back.Grid[0].ShouldBe(new[] { 1, 2 });
        back.Grid[1].ShouldBe(new[] { 3 });
        back.Groups["g"]["y"].ShouldBe(2);
        back.Raw.Items.Count.ShouldBe(2);
    }
}
