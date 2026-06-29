using System.Collections.Generic;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Converters;

internal static class Avm1TypeShape
{
    public static bool IsNumeric(Type type)
    {
        return Type.GetTypeCode(type) is TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16
            or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64
            or TypeCode.Single or TypeCode.Double or TypeCode.Decimal;
    }

    public static bool IsPassthrough(Type type)
    {
        return type == typeof(Avm1Value) || type == typeof(Avm1Object) || type == typeof(Avm1Array);
    }

    public static bool IsCollection(Type type)
    {
        return (type.IsArray && type.GetArrayRank() == 1)
            || TryGetDictionaryValue(type, out _)
            || TryGetEnumerableElement(type, out _);
    }

    public static bool TryGetDictionaryValue(Type type, out Type value)
    {
        foreach (var candidate in Self(type))
        {
            if (candidate.IsGenericType
                && candidate.GetGenericArguments() is { Length: 2 } args
                && args[0] == typeof(string))
            {
                var definition = candidate.GetGenericTypeDefinition();
                if (definition == typeof(IDictionary<,>) || definition == typeof(IReadOnlyDictionary<,>))
                {
                    value = args[1];
                    return true;
                }
            }
        }

        value = null!;
        return false;
    }

    public static bool TryGetEnumerableElement(Type type, out Type element)
    {
        foreach (var candidate in Self(type))
        {
            if (candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                element = candidate.GetGenericArguments()[0];
                return true;
            }
        }

        element = null!;
        return false;
    }

    private static IEnumerable<Type> Self(Type type)
    {
        yield return type;

        foreach (var contract in type.GetInterfaces())
            yield return contract;
    }
}
