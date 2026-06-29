namespace ShockwaveFlash.Avm1.Serialization.Metadata;

public sealed class Avm1ObjectInfoValues<T>
{
    public Func<object>? ObjectCreator { get; init; }

    public Func<object?[], object>? ConstructorFactory { get; init; }

    public string[]? ConstructorArguments { get; init; }

    public Func<Avm1SerializerOptions, Avm1PropertyInfo[]>? PropertyMetadataInitializer { get; init; }

    public string[]? BindingPath { get; init; }
}
