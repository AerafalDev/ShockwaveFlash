using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Metadata;

public abstract class Avm1SerializerContext : IAvm1TypeInfoResolver
{
    public Avm1SerializerOptions Options { get; }

    protected Avm1SerializerContext(Avm1SerializerOptions? options)
    {
        Options = options ?? new Avm1SerializerOptions();
        Options.TypeInfoResolver ??= Avm1TypeInfoResolver.Combine(this, new DefaultAvm1TypeInfoResolver());
    }

    public abstract Avm1TypeInfo? GetTypeInfo(Type type);

    public Avm1TypeInfo<T> GetTypeInfo<T>()
    {
        if (GetTypeInfo(typeof(T)) is not Avm1TypeInfo<T> info)
            throw new Avm1SerializationException($"Context '{GetType()}' has no AVM1 metadata for type '{typeof(T)}'.");

        info.EnsurePopulated();
        return info;
    }

    public T? Read<T>(Avm1Object source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var info = GetTypeInfo<T>();
        Avm1Value current = source;

        if (info.BindingPath is { } path)
            foreach (var segment in path)
            {
                if (current is not Avm1Object table || !table.Members.TryGetValue(segment, out var next))
                    return default;

                current = next;
            }

        return Avm1Serializer.Deserialize(current, info);
    }

    public void Write<T>(Avm1Object destination, T value)
    {
        ArgumentNullException.ThrowIfNull(destination);

        var info = GetTypeInfo<T>();
        var serialized = Avm1Serializer.Serialize(value, info);

        if (info.BindingPath is not { Length: > 0 } path)
            throw new Avm1SerializationException($"Type '{typeof(T)}' has no binding path to write into a globals object.");

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

    Avm1TypeInfo? IAvm1TypeInfoResolver.GetTypeInfo(Type type, Avm1SerializerOptions options)
    {
        return GetTypeInfo(type);
    }
}
