using System.Diagnostics.CodeAnalysis;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization;

public static class Avm1Serializer
{
    public static Avm1Value Serialize<T>(T value, Avm1TypeInfo<T> typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        var options = typeInfo.Options ?? Avm1SerializerOptions.Default;
        return typeInfo.Converter.WriteBoxed(value, options);
    }

    public static T Deserialize<T>(Avm1Value value, Avm1TypeInfo<T> typeInfo)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(typeInfo);
        var options = typeInfo.Options ?? Avm1SerializerOptions.Default;
        return (T)typeInfo.Converter.ReadBoxed(value, options)!;
    }

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

    [RequiresUnreferencedCode("AVM1 reflection serialization may require types that cannot be statically analyzed.")]
    [RequiresDynamicCode("AVM1 reflection serialization may construct converters at runtime.")]
    public static T? ReadGlobal<T>(Avm1Object globals, Avm1SerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(globals);
        options ??= Avm1SerializerOptions.Default;
        return Avm1GlobalBinding.Read(globals, (Avm1TypeInfo<T>)options.GetTypeInfo(typeof(T)));
    }

    [RequiresUnreferencedCode("AVM1 reflection serialization may require types that cannot be statically analyzed.")]
    [RequiresDynamicCode("AVM1 reflection serialization may construct converters at runtime.")]
    public static void WriteGlobal<T>(Avm1Object globals, T value, Avm1SerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(globals);
        options ??= Avm1SerializerOptions.Default;
        Avm1GlobalBinding.Write(globals, value, (Avm1TypeInfo<T>)options.GetTypeInfo(typeof(T)));
    }
}
