using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Converters;

internal sealed class Avm1PolymorphicConverter<TBase> : Avm1Converter
{
    public override Type Type =>
        typeof(TBase);

    internal override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options)
    {
        var polymorphism = options.GetTypeInfo(typeof(TBase)).Polymorphism!;

        if (value is not Avm1Object table)
            throw new Avm1SerializationException($"Cannot deserialize polymorphic '{typeof(TBase)}' from {value.GetType().Name}.");

        if (!table.Members.TryGetValue(polymorphism.DiscriminatorName, out var raw))
            throw new Avm1SerializationException($"Polymorphic AVM1 object for '{typeof(TBase)}' is missing the '{polymorphism.DiscriminatorName}' discriminator.");

        var discriminator = raw.AsString;
        if (!polymorphism.TryGetType(discriminator, out var derived))
            throw new Avm1SerializationException($"Unknown discriminator '{discriminator}' for '{typeof(TBase)}'.");

        return options.GetConverter(derived).ReadBoxed(value, options);
    }

    internal override Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options)
    {
        var polymorphism = options.GetTypeInfo(typeof(TBase)).Polymorphism!;
        var runtime = value!.GetType();

        if (!polymorphism.TryGetDiscriminator(runtime, out var discriminator))
            throw new Avm1SerializationException($"Type '{runtime}' is not a registered derived type of '{typeof(TBase)}'.");

        var tree = options.GetConverter(runtime).WriteBoxed(value, options);

        if (tree is Avm1Object table && !table.Members.ContainsKey(polymorphism.DiscriminatorName))
            table.Members[polymorphism.DiscriminatorName] = new Avm1String(discriminator);

        return tree;
    }
}
