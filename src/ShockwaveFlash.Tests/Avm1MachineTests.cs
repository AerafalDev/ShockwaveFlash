using ShockwaveFlash.Avm1;
using ShockwaveFlash.Avm1.Special;
using ShockwaveFlash.Avm1.Swf4;
using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class Avm1MachineTests
{
    [Fact]
    public void InitArray_preserves_element_order()
    {
        var machine = new Avm1Machine();

        machine.Execute(
        [
            new ActionPush([PushValue.String("arr")]),
            new ActionPush([PushValue.Integer(30)]),
            new ActionPush([PushValue.Integer(20)]),
            new ActionPush([PushValue.Integer(10)]),
            new ActionPush([PushValue.Integer(3)]),
            new ActionInitArray(),
            new ActionSetVariable(),
            new ActionEnd(),
        ]);

        var array = machine.Globals["arr"].AsArray;

        array.Items.Select(item => item.AsNumber).ShouldBe([10d, 20d, 30d]);
    }
}
