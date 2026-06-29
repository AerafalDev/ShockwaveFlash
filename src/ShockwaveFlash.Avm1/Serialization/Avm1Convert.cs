using System.Diagnostics.CodeAnalysis;
using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization;

public static class Avm1Convert
{
    public static T ReadGlobal<T>(Avm1Object globals)
        where T : IAvm1Serializable<T>
    {
        ArgumentNullException.ThrowIfNull(globals);

        var key = GlobalNameOrThrow<T>();

        if (!globals.Members.TryGetValue(key, out var value) || value is not Avm1Object table)
            throw new Avm1SerializationException($"AVM1 global '{key}' is missing or is not an object.");

        return T.FromAvm1Object(table);
    }

    public static bool TryReadGlobal<T>(Avm1Object globals, [NotNullWhen(true)] out T? value)
        where T : IAvm1Serializable<T>
    {
        value = default;

        if (globals is null || T.Avm1GlobalName is not { } key)
            return false;

        if (!globals.Members.TryGetValue(key, out var raw) || raw is not Avm1Object table)
            return false;

        return (value = T.FromAvm1Object(table)) is not null;
    }

    public static void WriteGlobal<T>(Avm1Object globals, T value)
        where T : IAvm1Serializable<T>
    {
        ArgumentNullException.ThrowIfNull(globals);

        if (value is null)
            throw new ArgumentNullException(nameof(value));

        globals.Members[GlobalNameOrThrow<T>()] = value.ToAvm1Object();
    }

    public static Dictionary<string, T> ReadMap<T>(Avm1Object table)
        where T : IAvm1Serializable<T>
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new Dictionary<string, T>(table.Members.Count, StringComparer.Ordinal);

        foreach (var (key, value) in table.Members)
        {
            if (value is Avm1Object entry)
                result[key] = T.FromAvm1Object(entry);
        }

        return result;
    }

    public static Avm1Object WriteMap<T>(IReadOnlyDictionary<string, T> map)
        where T : IAvm1Serializable<T>
    {
        ArgumentNullException.ThrowIfNull(map);

        var table = new Avm1Object();

        foreach (var (key, value) in map)
            table.Members[key] = value.ToAvm1Object();

        return table;
    }

    private static string GlobalNameOrThrow<T>()
        where T : IAvm1Serializable<T>
    {
        return T.Avm1GlobalName ?? throw new Avm1SerializationException($"Type '{typeof(T)}' has no AVM1 global name; annotate it with [Avm1Object(\"name\")] to bind it to a global.");
    }
}
