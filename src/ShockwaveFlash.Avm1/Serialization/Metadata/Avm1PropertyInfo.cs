namespace ShockwaveFlash.Avm1.Serialization.Metadata;

public sealed class Avm1PropertyInfo
{
    internal Avm1Converter Converter { get; set; } = null!;

    internal bool Nullable { get; set; }

    internal bool ThrowIfMissing { get; set; }

    internal bool IsValueScalar { get; set; }

    internal Type UnderlyingType { get; set; } = typeof(object);

    internal bool IsConstructorParameter { get; set; }

    internal bool Settable { get; set; }

    public string Name { get; set; } = string.Empty;

    public string MemberName { get; set; } = string.Empty;

    public Func<object, object?>? Get { get; set; }

    public Action<object, object?>? Set { get; set; }

    public bool IsRequired { get; set; }

    public int Order { get; set; }
}
