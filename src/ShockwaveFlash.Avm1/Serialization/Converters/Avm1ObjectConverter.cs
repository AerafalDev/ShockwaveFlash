using System.Collections.Generic;
using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Converters;

internal sealed class Avm1ObjectConverter<T> : Avm1Converter
{
    public override Type Type =>
        typeof(T);

    internal override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options)
    {
        var info = options.GetTypeInfo(typeof(T));
        info.EnsurePopulated();

        if (value is not Avm1Object table)
            throw new Avm1SerializationException($"Cannot deserialize '{typeof(T)}' from {value.GetType().Name}.");

        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        Avm1PropertyInfo? extension = null;

        foreach (var property in info.Properties)
        {
            if (property.IsExtensionData)
            {
                extension = property;
                continue;
            }

            values[property.MemberName] = ReadMember(property, table, options);
        }

        if (extension is not null)
            values[extension.MemberName] = CollectExtensionData(info, table);

        object instance;
        if (info.ConstructorFactory is not null)
        {
            var arguments = new object?[info.ConstructorArguments.Length];
            for (var i = 0; i < arguments.Length; i++)
                arguments[i] = values[info.ConstructorArguments[i]];
            instance = info.ConstructorFactory(arguments);
        }
        else
        {
            instance = info.ObjectFactory!();
        }

        foreach (var property in info.Properties)
            if (property.Set is not null && !property.IsConstructorParameter)
                property.Set(instance, values[property.MemberName]);

        return instance;
    }

    internal override Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options)
    {
        var info = options.GetTypeInfo(typeof(T));
        info.EnsurePopulated();

        var table = new Avm1Object();
        var instance = value!;
        var condition = options.DefaultIgnoreCondition;
        Avm1PropertyInfo? extension = null;

        foreach (var property in info.Properties)
        {
            if (property.IsExtensionData)
            {
                extension = property;
                continue;
            }

            var member = property.Get!(instance);

            if (member is null)
            {
                if (condition is Avm1IgnoreCondition.WhenWritingNull or Avm1IgnoreCondition.WhenWritingDefault or Avm1IgnoreCondition.Always)
                    continue;

                table.Members[property.Name] = Avm1Value.Null;
                continue;
            }

            if (condition is Avm1IgnoreCondition.Always)
                continue;
            if (condition is Avm1IgnoreCondition.WhenWritingDefault && property.IsValueScalar && member.Equals(Activator.CreateInstance(property.UnderlyingType)))
                continue;

            table.Members[property.Name] = property.Converter.WriteBoxed(member, options);
        }

        if (extension is not null && extension.Get!(instance) is IEnumerable<KeyValuePair<string, Avm1Value>> data)
            foreach (var pair in data)
                table.Members[pair.Key] = pair.Value;

        return table;
    }

    private static Dictionary<string, Avm1Value> CollectExtensionData(Avm1TypeInfo info, Avm1Object table)
    {
        var claimed = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in info.Properties)
            if (!property.IsExtensionData)
                claimed.Add(property.Name);

        var data = new Dictionary<string, Avm1Value>(StringComparer.Ordinal);
        foreach (var pair in table.Members)
            if (!claimed.Contains(pair.Key))
                data[pair.Key] = pair.Value;

        return data;
    }

    private static object? ReadMember(Avm1PropertyInfo property, Avm1Object table, Avm1SerializerOptions options)
    {
        table.Members.TryGetValue(property.Name, out var value);
        value ??= Avm1Value.Undefined;

        if (property.Nullable)
            return value.IsNull || value.IsUndefined ? null : property.Converter.ReadBoxed(value, options);

        if (value.IsNull || value.IsUndefined)
        {
            if (property.ThrowIfMissing)
                throw new Avm1SerializationException($"AVM1 object is missing required member '{property.Name}'.");
            if (property.IsValueScalar)
                return Activator.CreateInstance(property.UnderlyingType);

            return property.Converter.ReadBoxed(Avm1Value.Undefined, options);
        }

        return property.Converter.ReadBoxed(value, options);
    }
}
