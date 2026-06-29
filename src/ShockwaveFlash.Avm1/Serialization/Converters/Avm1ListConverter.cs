using System.Collections.Generic;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Converters;

internal sealed class Avm1ListConverter<TElement> : Avm1Converter
{
    private Avm1Converter? _element;

    public override Type Type => typeof(List<TElement>);

    internal override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options)
    {
        var element = _element ??= options.GetConverter(typeof(TElement));
        var list = new List<TElement>();

        if (value is Avm1Array array)
        {
            list.Capacity = array.Items.Count;

            foreach (var item in array.Items)
                list.Add((TElement)element.ReadBoxed(item, options)!);
        }

        return list;
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
