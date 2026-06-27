using System.Text;
using ShockwaveFlash.Avm1.Special;
using ShockwaveFlash.Avm1.Swf4;
using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1;

public static class Avm1Emitter
{
    private const int PoolByteBudget = 60000;

    public static ReadOnlyMemory<byte> EmitBytes(Avm1Object globals, byte swfVersion, IReadOnlyList<Action>? preamble = null)
    {
        return Action.EncodeCollection(Emit(globals, preamble), swfVersion);
    }

    public static IReadOnlyList<Action> Emit(Avm1Object globals, IReadOnlyList<Action>? preamble = null)
    {
        var pool = new List<string>();
        var indexByString = new Dictionary<string, int>(StringComparer.Ordinal);
        var poolBytes = 2;
        Collect(globals, pool, indexByString, ref poolBytes);

        var actions = new List<Action>();

        if (preamble is not null)
            foreach (var action in preamble)
                if (action is not ActionEnd)
                    actions.Add(action);

        if (pool.Count > 0)
            actions.Add(new ActionConstantPool(pool));

        foreach (var (name, value) in globals.Members)
        {
            actions.Add(PushString(name, indexByString));
            EmitValue(value, actions, indexByString);
            actions.Add(new ActionSetVariable());
        }

        actions.Add(new ActionEnd());
        return actions;
    }

    private static void Collect(Avm1Value value, List<string> pool, Dictionary<string, int> indexByString, ref int poolBytes)
    {
        switch (value)
        {
            case Avm1Object table:
                foreach (var (name, member) in table.Members)
                {
                    Intern(name, pool, indexByString, ref poolBytes);
                    Collect(member, pool, indexByString, ref poolBytes);
                }

                break;

            case Avm1Array list:
                foreach (var item in list.Items)
                    Collect(item, pool, indexByString, ref poolBytes);

                break;

            case Avm1String text:
                Intern(text.Value, pool, indexByString, ref poolBytes);
                break;

            default:
                break;
        }
    }

    private static void Intern(string value, List<string> pool, Dictionary<string, int> indexByString, ref int poolBytes)
    {
        if (indexByString.ContainsKey(value) || pool.Count >= ushort.MaxValue)
            return;

        var size = Encoding.UTF8.GetByteCount(value) + 1;
        if (poolBytes + size > PoolByteBudget)
            return;

        indexByString[value] = pool.Count;
        pool.Add(value);
        poolBytes += size;
    }

    private static void EmitValue(Avm1Value value, List<Action> actions, Dictionary<string, int> indexByString)
    {
        switch (value)
        {
            case Avm1Object table:
                foreach (var (name, member) in table.Members.Reverse())
                {
                    actions.Add(PushString(name, indexByString));
                    EmitValue(member, actions, indexByString);
                }

                actions.Add(PushInteger(table.Members.Count));
                actions.Add(new ActionInitObject());
                break;

            case Avm1Array list:
                for (var i = list.Items.Count - 1; i >= 0; i--)
                    EmitValue(list.Items[i], actions, indexByString);

                actions.Add(PushInteger(list.Items.Count));
                actions.Add(new ActionInitArray());
                break;

            case Avm1String text:
                actions.Add(PushString(text.Value, indexByString));
                break;

            default:
                actions.Add(new ActionPush([ToScalar(value)]));
                break;
        }
    }

    private static ActionPush PushString(string value, Dictionary<string, int> indexByString)
    {
        if (!indexByString.TryGetValue(value, out var index))
            return new ActionPush([PushValue.String(value)]);

        return index < 256
            ? new ActionPush([PushValue.Constant8((byte)index)])
            : new ActionPush([PushValue.Constant16((ushort)index)]);
    }

    private static ActionPush PushInteger(int value)
    {
        return new ActionPush([PushValue.Integer(value)]);
    }

    private static PushValue ToScalar(Avm1Value value)
    {
        return value switch
        {
            Avm1Boolean flag => PushValue.Boolean(flag.Value),
            Avm1Null => PushValue.Null(),
            Avm1Number number when number.Value == Math.Floor(number.Value) && number.Value is >= int.MinValue and <= int.MaxValue => PushValue.Integer((int)number.Value),
            Avm1Number number => PushValue.Double(number.Value),
            _ => PushValue.Undefined()
        };
    }
}
