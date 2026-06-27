using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Swf7;
using ShockwaveFlash.Avm1.Types;
using ShockwaveFlash.Exceptions;
using Shouldly;
using Avm1Action = ShockwaveFlash.Avm1.Action;

namespace ShockwaveFlash.Tests;

public sealed class Avm1CodecTests
{
    [Fact]
    public void Unknown_push_value_type_throws_a_typed_exception()
    {
        ReadOnlyMemory<byte> bytecode = new byte[] { 0x96, 0x01, 0x00, 0x0A };

        Should.Throw<SwfFormatException>(() => Avm1Action.DecodeCollection(bytecode, swfVersion: 6));
    }

    [Fact]
    public void An_action_shorter_than_its_declared_length_throws_a_typed_exception()
    {
        ReadOnlyMemory<byte> bytecode = new byte[] { 0x81, 0x03, 0x00, 0x05, 0x00, 0xFF };

        Should.Throw<SwfFormatException>(() => Avm1Action.DecodeCollection(bytecode, swfVersion: 6));
    }

    [Fact]
    public void With_owns_its_body_and_round_trips()
    {
        ReadOnlyMemory<byte> bytecode = new byte[] { 0x94, 0x02, 0x00, 0x02, 0x00, 0x06, 0x17 };

        var actions = Avm1Action.DecodeCollection(bytecode, swfVersion: 6);

        actions.Count.ShouldBe(1);
        actions[0].ShouldBeOfType<ActionWith>();

        Avm1Action.EncodeCollection(actions, swfVersion: 6).ToArray().ShouldBe(bytecode.ToArray());
    }

    [Fact]
    public void DefineFunction2_owns_its_body_and_round_trips()
    {
        var body = new byte[] { 0x06, 0x17 };
        var function = new ActionDefineFunction2("f", 0, (FunctionFlags)0, [], body);

        var encoded = Avm1Action.EncodeCollection([function], swfVersion: 6);
        var decoded = Avm1Action.DecodeCollection(encoded, swfVersion: 6);

        decoded.Count.ShouldBe(1);
        var roundTripped = decoded[0].ShouldBeOfType<ActionDefineFunction2>();
        roundTripped.Body.ToArray().ShouldBe(body);

        Avm1Action.EncodeCollection(decoded, swfVersion: 6).ToArray().ShouldBe(encoded.ToArray());
    }
}
