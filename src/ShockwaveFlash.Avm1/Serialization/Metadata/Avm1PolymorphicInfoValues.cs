namespace ShockwaveFlash.Avm1.Serialization.Metadata;

public sealed class Avm1PolymorphicInfoValues<TBase>
{
    public string DiscriminatorName { get; init; } = "$type";

    public (Type Type, string Discriminator)[] DerivedTypes { get; init; } = [];

    public string[]? BindingPath { get; init; }
}
