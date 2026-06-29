using System.Collections.Generic;

namespace ShockwaveFlash.Avm1.Serialization.Metadata;

internal sealed class Avm1PolymorphismInfo
{
    private readonly Dictionary<string, Type> _byDiscriminator = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, string> _byType = new();

    public string DiscriminatorName { get; init; } = "$type";

    public void Add(Type type, string discriminator)
    {
        _byDiscriminator[discriminator] = type;
        _byType[type] = discriminator;
    }

    public bool TryGetType(string discriminator, out Type type)
    {
        return _byDiscriminator.TryGetValue(discriminator, out type!);
    }

    public bool TryGetDiscriminator(Type type, out string discriminator)
    {
        return _byType.TryGetValue(type, out discriminator!);
    }
}
