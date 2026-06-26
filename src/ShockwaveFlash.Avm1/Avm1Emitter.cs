using ShockwaveFlash.Avm1.Special;
using ShockwaveFlash.Avm1.Swf4;
using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1;

public static class Avm1Emitter
{
    public static ReadOnlyMemory<byte> EmitBytes(IReadOnlyDictionary<string, object?> globals, byte swfVersion)
    {
        return Action.EncodeCollection(Emit(globals), swfVersion);
    }

    public static IReadOnlyList<Action> Emit(IReadOnlyDictionary<string, object?> globals)
    {
        var actions = new List<Action>();

        foreach (var (name, value) in globals)
        {
            actions.Add(PushString(name));
            EmitValue(value, actions);
            actions.Add(new ActionSetVariable());
        }

        actions.Add(new ActionEnd());
        return actions;
    }

    private static void EmitValue(object? value, List<Action> actions)
    {
        switch (value)
        {
            case IReadOnlyDictionary<string, object?> table:
                foreach (var (name, member) in table)
                {
                    actions.Add(PushString(name));
                    EmitValue(member, actions);
                }

                actions.Add(PushInteger(table.Count));
                actions.Add(new ActionInitObject());
                break;

            case IReadOnlyList<object?> list:
                foreach (var item in list)
                    EmitValue(item, actions);

                actions.Add(PushInteger(list.Count));
                actions.Add(new ActionInitArray());
                break;

            default:
                actions.Add(new ActionPush([ToPushValue(value)]));
                break;
        }
    }

    private static PushValue ToPushValue(object? value)
    {
        return value switch
        {
            null => PushValue.Null(),
            string text => PushValue.String(text),
            bool flag => PushValue.Boolean(flag),
            int integer => PushValue.Integer(integer),
            double number when number == Math.Floor(number) && number is >= int.MinValue and <= int.MaxValue => PushValue.Integer((int)number),
            double number => PushValue.Double(number),
            _ => PushValue.Undefined()
        };
    }

    private static ActionPush PushString(string value)
    {
        return new ActionPush([PushValue.String(value)]);
    }

    private static ActionPush PushInteger(int value)
    {
        return new ActionPush([PushValue.Integer(value)]);
    }
}
