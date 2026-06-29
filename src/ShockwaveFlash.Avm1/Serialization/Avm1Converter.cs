using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization;

public abstract class Avm1Converter
{
    public abstract Type Type { get; }

    internal Avm1Converter()
    {
    }

    public virtual bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == Type;
    }


    internal abstract object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options);

    internal abstract Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options);
}
