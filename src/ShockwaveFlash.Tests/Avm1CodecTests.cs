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
}
