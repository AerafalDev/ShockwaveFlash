using System.Diagnostics.CodeAnalysis;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization;

public static class Avm1Serializer
{
    [RequiresUnreferencedCode("AVM1 reflection serialization may require types that cannot be statically analyzed.")]
    [RequiresDynamicCode("AVM1 reflection serialization may construct converters at runtime.")]
    public static Avm1Value Serialize<T>(T value, Avm1SerializerOptions? options = null)
    {
        options ??= Avm1SerializerOptions.Default;
        return options.GetConverter(typeof(T)).WriteBoxed(value, options);
    }

    [RequiresUnreferencedCode("AVM1 reflection serialization may require types that cannot be statically analyzed.")]
    [RequiresDynamicCode("AVM1 reflection serialization may construct converters at runtime.")]
    public static T Deserialize<T>(Avm1Value value, Avm1SerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        options ??= Avm1SerializerOptions.Default;
        return (T)options.GetConverter(typeof(T)).ReadBoxed(value, options)!;
    }
}
