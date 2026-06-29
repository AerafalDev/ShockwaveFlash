using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization;

public abstract class Avm1Converter<T> : Avm1Converter
{
    public sealed override Type Type => typeof(T);

    public abstract T Read(Avm1Value value, Avm1SerializerOptions options);

    public abstract Avm1Value Write(T value, Avm1SerializerOptions options);

    internal sealed override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options) => Read(value, options);

    internal sealed override Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options) => Write((T)value!, options);
}
