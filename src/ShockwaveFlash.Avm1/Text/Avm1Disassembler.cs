using System.Globalization;
using System.Text;
using ShockwaveFlash.Avm1.Special;
using ShockwaveFlash.Avm1.Swf1;
using ShockwaveFlash.Avm1.Swf3;
using ShockwaveFlash.Avm1.Swf4;
using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Swf6;
using ShockwaveFlash.Avm1.Swf7;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Text;

public static class Avm1Disassembler
{
    public static string Disassemble(ReadOnlyMemory<byte> bytecode, byte swfVersion, Avm1DisassemblyKind kind = Avm1DisassemblyKind.Pcode)
    {
        return Disassemble(Action.DecodeCollection(bytecode, swfVersion), swfVersion, kind);
    }

    public static string Disassemble(IReadOnlyList<Action> actions, byte swfVersion, Avm1DisassemblyKind kind = Avm1DisassemblyKind.Pcode)
    {
        var builder = new StringBuilder();

        if (kind is Avm1DisassemblyKind.As2)
        {
            WriteAs2(builder, actions, swfVersion);
        }
        else
        {
            IReadOnlyList<string> pool = [];
            WritePcode(builder, actions, swfVersion, 0, ref pool);
        }

        return builder.ToString().TrimEnd('\n');
    }

    private static void WritePcode(StringBuilder builder, IReadOnlyList<Action> actions, byte swfVersion, int indent, ref IReadOnlyList<string> pool)
    {
        foreach (var action in actions)
        {
            if (action is ActionConstantPool constantPool)
            {
                pool = constantPool.Constants;
                Indent(builder, indent);
                builder.Append("ConstantPool ").Append(string.Join(", ", constantPool.Constants.Select(Quote))).Append('\n');
                continue;
            }

            if (action is ActionPush push)
            {
                foreach (var value in push.PushValues)
                {
                    Indent(builder, indent);
                    builder.Append("Push ").Append(FormatResolved(value, pool)).Append('\n');
                }

                continue;
            }

            Indent(builder, indent);
            builder.Append(PcodeHead(action));

            switch (action)
            {
                case ActionWith with:
                    WriteBlock(builder, with.Body, swfVersion, indent, ref pool);
                    break;

                case ActionDefineFunction function:
                    WriteBlock(builder, function.Body, swfVersion, indent, ref pool);
                    break;

                case ActionDefineFunction2 function:
                    WriteBlock(builder, function.Body, swfVersion, indent, ref pool);
                    break;

                case ActionTry tryAction:
                    WriteTryBlocks(builder, tryAction, swfVersion, indent, ref pool);
                    break;
            }

            builder.Append('\n');
        }
    }

    private static void WriteBlock(StringBuilder builder, ReadOnlyMemory<byte> body, byte swfVersion, int indent, ref IReadOnlyList<string> pool)
    {
        builder.Append(" {\n");
        WritePcode(builder, Action.DecodeCollection(body, swfVersion), swfVersion, indent + 1, ref pool);
        Indent(builder, indent);
        builder.Append('}');
    }

    private static void WriteTryBlocks(StringBuilder builder, ActionTry action, byte swfVersion, int indent, ref IReadOnlyList<string> pool)
    {
        WriteBlock(builder, action.TryBody, swfVersion, indent, ref pool);

        if (action.CatchBody.Length > 0)
        {
            var name = action.Flags.HasFlag(TryFlags.CatchInRegister) ? $"r{action.CatchRegister}" : action.CatchVariable;
            builder.Append(" Catch(").Append(name).Append(')');
            WriteBlock(builder, action.CatchBody, swfVersion, indent, ref pool);
        }

        if (action.FinallyBody.Length > 0)
        {
            builder.Append(" Finally");
            WriteBlock(builder, action.FinallyBody, swfVersion, indent, ref pool);
        }
    }

    private static string PcodeHead(Action action)
    {
        return action switch
        {
            ActionGotoFrame a => $"GotoFrame {a.Frame}",
            ActionGetURL a => $"GetURL {Quote(a.Url)}, {Quote(a.Target)}",
            ActionStoreRegister a => $"StoreRegister {a.RegisterNumber}",
            ActionWaitForFrame a => $"WaitForFrame {a.Frame}, {a.SkipCount}",
            ActionWaitForFrame2 a => $"WaitForFrame2 {a.SkipCount}",
            ActionSetTarget a => $"SetTarget {Quote(a.TargetName)}",
            ActionGoToLabel a => $"GoToLabel {Quote(a.Label)}",
            ActionJump a => $"Jump {a.BranchOffset}",
            ActionIf a => $"If {a.BranchOffset}",
            ActionGotoFrame2 a => a.HasSceneBias ? $"GotoFrame2 {(a.Play ? "play" : "stop")}, {a.SceneBias}" : $"GotoFrame2 {(a.Play ? "play" : "stop")}",
            ActionWith => "With",
            ActionDefineFunction a => $"DefineFunction {Quote(a.Name)}({string.Join(", ", a.Parameters)})",
            ActionDefineFunction2 a => $"DefineFunction2 {Quote(a.Name)}({string.Join(", ", a.Parameters.Select(p => p.Name))})",
            ActionTry => "Try",
            ActionUnknown a => $"Unknown(0x{(byte)a.Opcode:X2})",
            _ => action.Opcode.ToString()
        };
    }

    private static void WriteAs2(StringBuilder builder, IReadOnlyList<Action> actions, byte swfVersion)
    {
        var stack = new Stack<string>();
        IReadOnlyList<string> pool = [];

        foreach (var action in actions)
        {
            var op = BinaryOperator(action);

            if (op is not null)
            {
                var right = Pop(stack);
                var left = Pop(stack);
                stack.Push($"({left} {op} {right})");
                continue;
            }

            switch (action)
            {
                case ActionConstantPool constantPool:
                    pool = constantPool.Constants;
                    continue;

                case ActionPush push:
                    foreach (var value in push.PushValues)
                        stack.Push(FormatResolved(value, pool));
                    continue;

                case ActionGetVariable:
                    stack.Push(Identifier(Pop(stack)));
                    continue;

                case ActionSetVariable:
                {
                    var value = Pop(stack);
                    var name = Identifier(Pop(stack));
                    builder.Append(name).Append(" = ").Append(value).Append(";\n");
                    continue;
                }

                case ActionGetMember:
                {
                    var name = Pop(stack);
                    var member = Member(Pop(stack), name);
                    stack.Push(member);
                    continue;
                }

                case ActionSetMember:
                {
                    var value = Pop(stack);
                    var name = Pop(stack);
                    var member = Member(Pop(stack), name);
                    builder.Append(member).Append(" = ").Append(value).Append(";\n");
                    continue;
                }

                case ActionInitObject:
                {
                    var count = ParseCount(Pop(stack));
                    var pairs = new List<string>();

                    for (var i = 0; i < count; i++)
                    {
                        var value = Pop(stack);
                        var name = Identifier(Pop(stack));
                        pairs.Add($"{name}: {value}");
                    }

                    pairs.Reverse();
                    stack.Push("{" + string.Join(", ", pairs) + "}");
                    continue;
                }

                case ActionInitArray:
                {
                    var count = ParseCount(Pop(stack));
                    var items = new List<string>();

                    for (var i = 0; i < count; i++)
                        items.Add(Pop(stack));

                    stack.Push("[" + string.Join(", ", items) + "]");
                    continue;
                }

                case ActionNewObject:
                {
                    var name = Identifier(Pop(stack));
                    var arguments = PopArguments(stack);
                    stack.Push($"new {name}({arguments})");
                    continue;
                }

                case ActionNewMethod:
                {
                    var name = Pop(stack);
                    var target = Pop(stack);
                    var arguments = PopArguments(stack);
                    stack.Push($"new {Member(target, name)}({arguments})");
                    continue;
                }

                case ActionCallFunction:
                {
                    var name = Identifier(Pop(stack));
                    var arguments = PopArguments(stack);
                    stack.Push($"{name}({arguments})");
                    continue;
                }

                case ActionCallMethod:
                {
                    var name = Pop(stack);
                    var target = Pop(stack);
                    var arguments = PopArguments(stack);
                    stack.Push(Identifier(name).Length is 0 || name is "undefined" ? $"{target}({arguments})" : $"{Member(target, name)}({arguments})");
                    continue;
                }

                case ActionNot:
                    stack.Push("!" + Pop(stack));
                    continue;

                case ActionTrace:
                    builder.Append("trace(").Append(Pop(stack)).Append(");\n");
                    continue;

                case ActionPop:
                    builder.Append(Pop(stack)).Append(";\n");
                    continue;

                case ActionEnd:
                    continue;
            }

            Flush(builder, stack);
            builder.Append(PcodeHead(action)).Append('\n');
        }

        Flush(builder, stack);
    }

    private static string? BinaryOperator(Action action)
    {
        return action switch
        {
            ActionAdd or ActionAdd2 or ActionStringAdd => "+",
            ActionSubtract => "-",
            ActionMultiply => "*",
            ActionDivide => "/",
            ActionModulo => "%",
            ActionEquals or ActionEquals2 or ActionStringEquals => "==",
            ActionLess or ActionLess2 or ActionStringLess => "<",
            ActionGreater or ActionStringGreater => ">",
            ActionAnd => "&&",
            ActionOr => "||",
            ActionBitAnd => "&",
            ActionBitOr => "|",
            ActionBitXor => "^",
            _ => null
        };
    }

    private static void Flush(StringBuilder builder, Stack<string> stack)
    {
        while (stack.Count > 0)
            builder.Append(stack.Pop()).Append(";\n");
    }

    private static string Pop(Stack<string> stack)
    {
        return stack.Count > 0 ? stack.Pop() : "undefined";
    }

    private static string Member(string target, string name)
    {
        var identifier = Identifier(name);

        return IsIdentifier(identifier) ? $"{target}.{identifier}" : $"{target}[{name}]";
    }

    private static int ParseCount(string value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) ? count : 0;
    }

    private static string PopArguments(Stack<string> stack)
    {
        var count = ParseCount(Pop(stack));
        var arguments = new List<string>();

        for (var i = 0; i < count; i++)
            arguments.Add(Pop(stack));

        return string.Join(", ", arguments);
    }

    private static string FormatResolved(PushValue value, IReadOnlyList<string> pool)
    {
        return value switch
        {
            PushValue.PushValueConstant8 x => x.ConstantIndex < pool.Count ? Quote(pool[x.ConstantIndex]) : $"c{x.ConstantIndex}",
            PushValue.PushValueConstant16 x => x.ConstantIndex < pool.Count ? Quote(pool[x.ConstantIndex]) : $"c{x.ConstantIndex}",
            _ => FormatValue(value)
        };
    }

    private static string FormatValue(PushValue value)
    {
        return value switch
        {
            PushValue.PushValueString x => Quote(x.Value),
            PushValue.PushValueFloat x => x.Value.ToString(CultureInfo.InvariantCulture),
            PushValue.PushValueDouble x => x.Value.ToString(CultureInfo.InvariantCulture),
            PushValue.PushValueInteger x => x.Value.ToString(CultureInfo.InvariantCulture),
            PushValue.PushValueBoolean x => x.Value ? "true" : "false",
            PushValue.PushValueNull => "null",
            PushValue.PushValueUndefined => "undefined",
            PushValue.PushValueRegister x => $"r{x.RegisterIndex}",
            PushValue.PushValueConstant8 x => $"c{x.ConstantIndex}",
            PushValue.PushValueConstant16 x => $"c{x.ConstantIndex}",
            _ => "undefined"
        };
    }

    private static string Identifier(string value)
    {
        if (value.Length >= 2 && value[0] is '"' && value[^1] is '"')
            return value[1..^1].Replace("\\\"", "\"").Replace("\\\\", "\\");

        return value;
    }

    private static bool IsIdentifier(string value)
    {
        if (value.Length is 0 || (!char.IsLetter(value[0]) && value[0] is not '_'))
            return false;

        foreach (var character in value)
            if (!char.IsLetterOrDigit(character) && character is not '_')
                return false;

        return true;
    }

    private static string Quote(string value)
    {
        return $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
    }

    private static void Indent(StringBuilder builder, int indent)
    {
        builder.Append(' ', indent * 2);
    }
}
