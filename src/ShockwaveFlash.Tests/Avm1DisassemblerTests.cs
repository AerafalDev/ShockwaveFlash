using ShockwaveFlash.Avm1.Special;
using ShockwaveFlash.Avm1.Swf4;
using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Text;
using ShockwaveFlash.Avm1.Types;
using Shouldly;
using Avm1Action = ShockwaveFlash.Avm1.Action;

namespace ShockwaveFlash.Tests;

public sealed class Avm1DisassemblerTests
{
    [Fact]
    public void Pcode_lists_opcodes_and_operands()
    {
        IReadOnlyList<Avm1Action> actions =
        [
            new ActionPush([PushValue.String("name")]),
            new ActionPush([PushValue.String("value")]),
            new ActionSetVariable(),
            new ActionEnd(),
        ];

        var text = Avm1Disassembler.Disassemble(actions, swfVersion: 6, Avm1DisassemblyKind.Pcode);

        text.ShouldBe("Push \"name\"\nPush \"value\"\nSetVariable\nEnd");
    }

    [Fact]
    public void As2_reconstructs_a_variable_assignment()
    {
        IReadOnlyList<Avm1Action> actions =
        [
            new ActionPush([PushValue.String("name")]),
            new ActionPush([PushValue.String("value")]),
            new ActionSetVariable(),
            new ActionEnd(),
        ];

        var text = Avm1Disassembler.Disassemble(actions, swfVersion: 6, Avm1DisassemblyKind.As2);

        text.ShouldBe("name = \"value\";");
    }

    [Fact]
    public void Pcode_resolves_constant_pool_references()
    {
        IReadOnlyList<Avm1Action> actions =
        [
            new ActionConstantPool(["greeting"]),
            new ActionPush([PushValue.Constant8(0)]),
        ];

        var text = Avm1Disassembler.Disassemble(actions, swfVersion: 6, Avm1DisassemblyKind.Pcode);

        text.ShouldBe("ConstantPool \"greeting\"\nPush \"greeting\"");
    }

    [Fact]
    public void Pcode_indents_a_nested_with_body()
    {
        ReadOnlyMemory<byte> bytecode = new byte[] { 0x94, 0x02, 0x00, 0x02, 0x00, 0x06, 0x17 };

        var text = Avm1Disassembler.Disassemble(bytecode, swfVersion: 6, Avm1DisassemblyKind.Pcode);

        text.ShouldBe("With {\n  Play\n  Pop\n}");
    }

    [Fact]
    public void As2_renders_a_named_function_with_its_body()
    {
        var body = Avm1Action.EncodeCollection(
        [
            new ActionPush([PushValue.String("hi")]),
            new ActionTrace(),
        ], swfVersion: 6);

        IReadOnlyList<Avm1Action> actions = [new ActionDefineFunction("f", ["a"], body)];

        var text = Avm1Disassembler.Disassemble(actions, swfVersion: 6, Avm1DisassemblyKind.As2);

        text.ShouldBe("function f(a) {\n  trace(\"hi\");\n}");
    }

    [Fact]
    public void As2_resolves_a_stored_register()
    {
        IReadOnlyList<Avm1Action> actions =
        [
            new ActionPush([PushValue.String("hi")]),
            new ActionStoreRegister(0),
            new ActionPop(),
            new ActionPush([PushValue.String("name")]),
            new ActionPush([PushValue.Register(0)]),
            new ActionSetVariable(),
        ];

        var text = Avm1Disassembler.Disassemble(actions, swfVersion: 6, Avm1DisassemblyKind.As2);

        text.ShouldBe("\"hi\";\nname = \"hi\";");
    }
}
