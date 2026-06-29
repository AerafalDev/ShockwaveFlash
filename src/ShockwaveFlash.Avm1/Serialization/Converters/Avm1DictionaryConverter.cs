using System.Collections.Generic;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Converters;

internal sealed class Avm1DictionaryConverter<TValue> : Avm1Converter
{
    private Avm1Converter? _value;

    public override Type Type => typeof(Dictionary<string, TValue>);

    internal override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options)
    {
        var converter = _value ??= options.GetConverter(typeof(TValue));
        var result = new Dictionary<string, TValue>(StringComparer.Ordinal);

        if (value is Avm1Object table)
        {
            foreach (var (key, item) in table.Members)
                result[key] = (TValue)converter.ReadBoxed(item, options)!;
        }

        return result;
    }

    internal override Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options)
    {
        var converter = _value ??= options.GetConverter(typeof(TValue));
        var table = new Avm1Object();

        foreach (var (key, item) in (IEnumerable<KeyValuePair<string, TValue>>)value!)
            table.Members[key] = converter.WriteBoxed(item, options);

        return table;
    }
}
