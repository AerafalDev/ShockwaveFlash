namespace ShockwaveFlash.Avm1.Serialization.Metadata;

public interface IAvm1TypeInfoResolver
{
    Avm1TypeInfo? GetTypeInfo(Type type, Avm1SerializerOptions options);
}
