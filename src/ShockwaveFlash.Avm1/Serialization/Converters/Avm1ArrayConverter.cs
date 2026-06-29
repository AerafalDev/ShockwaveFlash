using System.Collections.Generic;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Converters;

internal sealed class Avm1ArrayConverter<TElement> : Avm1Converter
{
    private Avm1Converter? _element;

    public override Type Type => typeof(TElement[]);

    internal override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options)
    {
        var element = _element ??= options.GetConverter(typeof(TElement));

        if (value is not Avm1Array array)
            return Array.Empty<TElement>();

        var result = new TElement[array.Items.Count];

        for (var i = 0; i < array.Items.Count; i++)
            result[i] = (TElement)element.ReadBoxed(array.Items[i], options)!;

        return result;
    }

    internal override Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options)
    {
        var element = _element ??= options.GetConverter(typeof(TElement));
        var array = new Avm1Array();

        foreach (var item in (IEnumerable<TElement>)value!)
            array.Items.Add(element.WriteBoxed(item, options));

        return array;
    }
}
