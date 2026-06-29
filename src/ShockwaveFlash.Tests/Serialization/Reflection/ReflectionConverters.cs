using System.Globalization;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

public sealed class CoordConverter : Avm1Converter<Coord>
{
    public override Coord Read(Avm1Value value, Avm1SerializerOptions options)
    {
        var parts = value.AsString.Split(',');
        return new Coord(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture));
    }

    public override Avm1Value Write(Coord value, Avm1SerializerOptions options)
    {
        return new Avm1String($"{value.X},{value.Y}");
    }
}

public sealed class VectorConverter : Avm1Converter<Vector>
{
    public override Vector Read(Avm1Value value, Avm1SerializerOptions options)
    {
        var parts = value.AsString.Split(',');
        return new Vector(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture));
    }

    public override Avm1Value Write(Vector value, Avm1SerializerOptions options)
    {
        return new Avm1String($"{value.X},{value.Y}");
    }
}

public sealed class TaggedTypeConverter : Avm1Converter<Tagged>
{
    public override Tagged Read(Avm1Value value, Avm1SerializerOptions options) => new(Parse(value));

    public override Avm1Value Write(Tagged value, Avm1SerializerOptions options) => new Avm1String($"type:{value.N}");

    internal static int Parse(Avm1Value value) => int.Parse(value.AsString.Split(':')[1], CultureInfo.InvariantCulture);
}

public sealed class TaggedPropConverter : Avm1Converter<Tagged>
{
    public override Tagged Read(Avm1Value value, Avm1SerializerOptions options) => new(TaggedTypeConverter.Parse(value));

    public override Avm1Value Write(Tagged value, Avm1SerializerOptions options) => new Avm1String($"prop:{value.N}");
}

public sealed class TaggedOptionsConverter : Avm1Converter<Tagged>
{
    public override Tagged Read(Avm1Value value, Avm1SerializerOptions options) => new(TaggedTypeConverter.Parse(value));

    public override Avm1Value Write(Tagged value, Avm1SerializerOptions options) => new Avm1String($"opt:{value.N}");
}

public sealed class CoordFactory : Avm1ConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Coord);

    public override Avm1Converter CreateConverter(Type typeToConvert, Avm1SerializerOptions options) => new CoordConverter();
}
