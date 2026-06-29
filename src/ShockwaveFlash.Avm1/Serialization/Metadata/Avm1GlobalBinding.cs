using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Metadata;

internal static class Avm1GlobalBinding
{
    public static T? Read<T>(Avm1Object source, Avm1TypeInfo<T> typeInfo)
    {
        Avm1Value current = source;

        if (typeInfo.BindingPath is { } path)
            foreach (var segment in path)
            {
                if (current is not Avm1Object table || !table.Members.TryGetValue(segment, out var next))
                    return default;

                current = next;
            }

        return Avm1Serializer.Deserialize(current, typeInfo);
    }

    public static void Write<T>(Avm1Object destination, T value, Avm1TypeInfo<T> typeInfo)
    {
        var serialized = Avm1Serializer.Serialize(value, typeInfo);

        if (typeInfo.BindingPath is not { Length: > 0 } path)
            throw new Avm1SerializationException($"Type '{typeof(T)}' has no binding path; annotate it with [Avm1Object(\"name\")] or register it with [Avm1Serializable(typeof({typeof(T).Name}), \"name\")].");

        var current = destination;
        for (var i = 0; i < path.Length - 1; i++)
        {
            if (current.Members.TryGetValue(path[i], out var next) && next is Avm1Object child)
            {
                current = child;
            }
            else
            {
                child = new Avm1Object();
                current.Members[path[i]] = child;
                current = child;
            }
        }

        current.Members[path[^1]] = serialized;
    }
}
