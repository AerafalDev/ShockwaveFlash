using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;
using ShockwaveFlash.Tests.Models;
using ShockwaveFlash.Tests.Serialization;
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

        var typeInfo = TestModelsContext.Default.Nested;
        var obj = Avm1Serializer.Serialize(model, typeInfo).AsObject;

        obj.Members.ShouldNotContainKey("opt");

        var back = Avm1Serializer.Deserialize(obj, typeInfo);

        Avm1Trees.DeepEquals(obj, Avm1Serializer.Serialize(back, typeInfo)).ShouldBeTrue();
        back.Flags["a"].ShouldBe(new[] { true, false });
        back.Grid[0].ShouldBe(new[] { 1, 2 });
        back.Grid[1].ShouldBe(new[] { 3 });
        back.Groups["g"]["y"].ShouldBe(2);
        back.Raw.Items.Count.ShouldBe(2);
    }
}
