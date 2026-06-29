using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization;

public abstract class Avm1ConverterFactory : Avm1Converter
{
    public sealed override Type Type =>
        typeof(object);

    public abstract Avm1Converter CreateConverter(Type typeToConvert, Avm1SerializerOptions options);

    internal sealed override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options)
    {
        throw new NotSupportedException();
    }


    internal sealed override Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options)
    {
        throw new NotSupportedException();
    }

}
