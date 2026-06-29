namespace ShockwaveFlash.Avm1.Serialization.Metadata;

public sealed class Avm1PropertyInfoValues<T>
{
    public string MemberName { get; init; } = "";

    public string Avm1PropertyName { get; init; } = "";

    public Type MemberType { get; init; } = typeof(object);

    public Func<object, object?>? Getter { get; init; }

    public Action<object, object?>? Setter { get; init; }

    public bool Nullable { get; init; }

    public bool ThrowIfMissing { get; init; }

    public bool IsValueScalar { get; init; }

    public bool IsConstructorParameter { get; init; }

    public int Order { get; init; }

    public bool IsExtensionData { get; init; }

    public Type? ConverterType { get; init; }
}
