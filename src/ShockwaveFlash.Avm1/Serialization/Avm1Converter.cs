using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization;

public abstract class Avm1Converter
{
    internal Avm1Converter()
    {
    }

    public abstract Type Type { get; }

    public virtual bool CanConvert(Type typeToConvert) => typeToConvert == Type;

    internal abstract object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options);

    internal abstract Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options);
}
