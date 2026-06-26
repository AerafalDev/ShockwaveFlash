using System.Globalization;
using ShockwaveFlash.Avm1.Special;
using ShockwaveFlash.Avm1.Swf4;
using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1;

public sealed class Avm1Machine
{
    private static readonly object s_undefined = new();

    private readonly Stack<object?> _stack = new();

    private readonly Dictionary<string, object?> _globals = new(StringComparer.Ordinal);

    private IReadOnlyList<string> _constantPool = [];

    public static object Undefined => s_undefined;

    public IReadOnlyDictionary<string, object?> Globals => _globals;

    public static IReadOnlyDictionary<string, object?> Run(ReadOnlyMemory<byte> actions, byte swfVersion)
    {
        var machine = new Avm1Machine();
        machine.Execute(Action.DecodeCollection(actions, swfVersion));
        return machine.Globals;
    }

    public void Execute(IReadOnlyList<Action> actions)
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
                        _stack.Push(_globals.GetValueOrDefault(name, s_undefined));
                        break;
                    }

                case ActionSetVariable:
                    {
                        var value = Pop();
                        var name = ToStr(Pop());
                        _globals[name] = value;
                        break;
                    }

                case ActionGetMember:
                    {
                        var name = ToStr(Pop());
                        var target = Pop();
                        _stack.Push(target is Dictionary<string, object?> members && members.TryGetValue(name, out var value) ? value : s_undefined);
                        break;
                    }

                case ActionSetMember:
                    {
                        var value = Pop();
                        var name = ToStr(Pop());
                        if (Pop() is Dictionary<string, object?> members)
                            members[name] = value;
                        break;
                    }

                case ActionInitObject:
                    {
                        var count = (int)ToNum(Pop());
                        var members = new Dictionary<string, object?>(StringComparer.Ordinal);
                        for (var i = 0; i < count; i++)
                        {
                            var value = Pop();
                            members[ToStr(Pop())] = value;
                        }

                        _stack.Push(members);
                        break;
                    }

                case ActionInitArray:
                    {
                        var count = (int)ToNum(Pop());
                        var items = new List<object?>(count);
                        for (var i = 0; i < count; i++)
                            items.Add(Pop());

                        items.Reverse();
                        _stack.Push(items);
                        break;
                    }

                case ActionNewObject:
                    {
                        _ = ToStr(Pop());
                        DropArguments();
                        _stack.Push(new Dictionary<string, object?>(StringComparer.Ordinal));
                        break;
                    }

                case ActionCallMethod:
                    {
                        _ = ToStr(Pop());
                        _ = Pop();
                        DropArguments();
                        _stack.Push(s_undefined);
                        break;
                    }

                case ActionCallFunction:
                    {
                        _ = ToStr(Pop());
                        DropArguments();
                        _stack.Push(s_undefined);
                        break;
                    }

                case ActionPop:
                    Pop();
                    break;

                case ActionEnd:
                    return;

                default:
                    break;
            }
        }
    }

    private object? Resolve(PushValue value)
    {
        return value switch
        {
            PushValue.PushValueString item => item.Value,
            PushValue.PushValueInteger item => (double)item.Value,
            PushValue.PushValueFloat item => (double)item.Value,
            PushValue.PushValueDouble item => item.Value,
            PushValue.PushValueBoolean item => item.Value,
            PushValue.PushValueNull => null,
            PushValue.PushValueConstant8 item => Constant(item.ConstantIndex),
            PushValue.PushValueConstant16 item => Constant(item.ConstantIndex),
            _ => s_undefined
        };
    }

    private object? Constant(int index)
    {
        return index >= 0 && index < _constantPool.Count ? _constantPool[index] : s_undefined;
    }

    private object? Pop()
    {
        return _stack.Count > 0 ? _stack.Pop() : s_undefined;
    }

    private void DropArguments()
    {
        var count = (int)ToNum(Pop());
        for (var i = 0; i < count; i++)
            Pop();
    }

    private static double ToNum(object? value)
    {
        return value switch
        {
            double number => number,
            bool flag => flag ? 1 : 0,
            string text => double.TryParse(text, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0,
            _ => 0
        };
    }

    private static string ToStr(object? value)
    {
        return value switch
        {
            null => "null",
            string text => text,
            bool flag => flag ? "true" : "false",
            double number => FormatNumber(number),
            _ => ReferenceEquals(value, s_undefined) ? "undefined" : value.ToString() ?? "null"
        };
    }

    private static string FormatNumber(double value)
    {
        if (double.IsFinite(value) && value == Math.Floor(value) && Math.Abs(value) < 1e15)
            return ((long)value).ToString(CultureInfo.InvariantCulture);

        return value.ToString(CultureInfo.InvariantCulture);
    }
}
