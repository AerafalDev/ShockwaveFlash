namespace ShockwaveFlash.Avm1.Serialization.Metadata;

public abstract class Avm1SerializerContext : IAvm1TypeInfoResolver
{
    public Avm1SerializerOptions Options { get; }

    protected Avm1SerializerContext(Avm1SerializerOptions? options)
    {
        Options = options ?? new Avm1SerializerOptions();
    }

    public abstract Avm1TypeInfo? GetTypeInfo(Type type);

    Avm1TypeInfo? IAvm1TypeInfoResolver.GetTypeInfo(Type type, Avm1SerializerOptions options)
    {
        return GetTypeInfo(type);
    }
}
