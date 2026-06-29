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
        return Avm1GlobalBinding.Read(source, GetTypeInfo<T>());
    }

    public void Write<T>(Avm1Object destination, T value)
    {
        ArgumentNullException.ThrowIfNull(destination);
        Avm1GlobalBinding.Write(destination, value, GetTypeInfo<T>());
    }

    Avm1TypeInfo? IAvm1TypeInfoResolver.GetTypeInfo(Type type, Avm1SerializerOptions options)
    {
        return GetTypeInfo(type);
    }
}
