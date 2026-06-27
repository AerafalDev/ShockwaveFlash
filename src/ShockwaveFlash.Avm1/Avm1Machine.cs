using System.Globalization;
using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Special;
using ShockwaveFlash.Avm1.Swf4;
using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1;

public sealed class Avm1Machine
{
    private readonly Stack<Avm1Value> _stack = new();

    private readonly Avm1Object _globals = new();

    private readonly HashSet<ActionOpcode> _unsupported = [];

    private IReadOnlyList<string> _constantPool = [];

    public Avm1Object Globals => _globals;

    public IReadOnlySet<ActionOpcode> UnsupportedOpcodes => _unsupported;

    public static Avm1Object Run(ReadOnlyMemory<byte> actions, byte swfVersion, bool strict = false)
    {
        var machine = new Avm1Machine();
        machine.Execute(Action.DecodeCollection(actions, swfVersion), strict);
        return machine.Globals;
    }

    public void Execute(IReadOnlyList<Action> actions, bool strict = false)
    {
        foreach (var action in actions)
        {
            switch (action)
            {
                case ActionConstantPool constantPool:
                    _constantPool = constantPool.Constants;
                    break;

                case ActionPush push:
                    foreach (var value in push.PushValues)
                        _stack.Push(Resolve(value));
                    break;

                case ActionGetVariable:
                    {
                        var name = ToStr(Pop());
                        _stack.Push(_globals.Members.GetValueOrDefault(name, Avm1Value.Undefined));
                        break;
                    }

                case ActionSetVariable:
                    {
                        var value = Pop();
                        _globals.Members[ToStr(Pop())] = value;
                        break;
                    }

                case ActionGetMember:
                    {
                        var name = ToStr(Pop());
                        _stack.Push(Pop() is Avm1Object members && members.Members.TryGetValue(name, out var value) ? value : Avm1Value.Undefined);
                        break;
                    }

                case ActionSetMember:
                    {
                        var value = Pop();
                        var name = ToStr(Pop());
                        if (Pop() is Avm1Object members)
                            members.Members[name] = value;
                        break;
                    }

                case ActionInitObject:
                    {
                        var count = (int)ToNum(Pop());
                        var members = new Avm1Object();
                        for (var i = 0; i < count; i++)
                        {
                            var value = Pop();
                            members.Members[ToStr(Pop())] = value;
                        }

                        _stack.Push(members);
                        break;
                    }

                case ActionInitArray:
                    {
                        var count = (int)ToNum(Pop());
                        var items = new Avm1Array();
                        for (var i = 0; i < count; i++)
                            items.Items.Add(Pop());

                        _stack.Push(items);
                        break;
                    }

                case ActionNewObject:
                    {
                        _ = ToStr(Pop());
                        DropArguments();
                        _stack.Push(new Avm1Object());
                        break;
                    }

                case ActionCallMethod:
                    {
                        _ = ToStr(Pop());
                        _ = Pop();
                        DropArguments();
                        _stack.Push(Avm1Value.Undefined);
                        break;
                    }

                case ActionCallFunction:
                    {
                        _ = ToStr(Pop());
                        DropArguments();
                        _stack.Push(Avm1Value.Undefined);
                        break;
                    }

                case ActionAdd2:
                    {
                        var right = Pop();
                        var left = Pop();
                        _stack.Push(left is Avm1String || right is Avm1String
                            ? (Avm1Value)(ToStr(left) + ToStr(right))
                            : ToNum(left) + ToNum(right));
                        break;
                    }

                case ActionStringAdd:
                    {
                        var right = Pop();
                        var left = Pop();
                        _stack.Push(ToStr(left) + ToStr(right));
                        break;
                    }

                case ActionPop:
                    Pop();
                    break;

                case ActionEnd:
                    return;

                default:
                    if (strict)
                        throw new Avm1UnsupportedActionException(action.Opcode);
                    _unsupported.Add(action.Opcode);
                    break;
            }
        }
    }

    private Avm1Value Resolve(PushValue value)
    {
        return value switch
        {
            PushValue.PushValueString item => item.Value,
            PushValue.PushValueInteger item => item.Value,
            PushValue.PushValueFloat item => (double)item.Value,
            PushValue.PushValueDouble item => item.Value,
            PushValue.PushValueBoolean item => item.Value,
            PushValue.PushValueNull => Avm1Value.Null,
            PushValue.PushValueConstant8 item => Constant(item.ConstantIndex),
            PushValue.PushValueConstant16 item => Constant(item.ConstantIndex),
            _ => Avm1Value.Undefined
        };
    }

    private Avm1Value Constant(int index)
    {
        return index >= 0 && index < _constantPool.Count ? _constantPool[index] : Avm1Value.Undefined;
    }

    private Avm1Value Pop()
    {
        return _stack.Count > 0 ? _stack.Pop() : Avm1Value.Undefined;
    }

    private void DropArguments()
    {
        var count = (int)ToNum(Pop());
        for (var i = 0; i < count; i++)
            Pop();
    }

    private static double ToNum(Avm1Value value)
    {
        return value switch
        {
            Avm1Number number => number.Value,
            Avm1Boolean flag => flag.Value ? 1 : 0,
            Avm1String text => double.TryParse(text.Value, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0,
            _ => 0
        };
    }

    private static string ToStr(Avm1Value value)
    {
        return value switch
        {
            Avm1String text => text.Value,
            Avm1Number number => FormatNumber(number.Value),
            Avm1Boolean flag => flag.Value ? "true" : "false",
            Avm1Null => "null",
            Avm1Undefined => "undefined",
            _ => value.ToString() ?? "null"
        };
    }

    private static string FormatNumber(double value)
    {
        if (double.IsFinite(value) && value == Math.Floor(value) && Math.Abs(value) < 1e15)
            return ((long)value).ToString(CultureInfo.InvariantCulture);

        return value.ToString(CultureInfo.InvariantCulture);
    }
}
