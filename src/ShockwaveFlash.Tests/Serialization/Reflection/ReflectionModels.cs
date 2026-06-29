using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

public enum Tier
{
    Free,
    Pro,
    Max,
}

public sealed record Coord(int X, int Y);

[Avm1Converter(typeof(VectorConverter))]
public sealed record Vector(int X, int Y);

[Avm1Converter(typeof(TaggedTypeConverter))]
public sealed record Tagged(int N);

public sealed record Numbers(
    byte B,
    sbyte Sb,
    short S,
    ushort Us,
    int I,
    uint Ui,
    long L,
    ulong Ul,
    float F,
    double D,
    decimal M,
    bool Bo,
    Tier E);

public sealed record Optionals(int? A, string? Name, List<int>? Nums, Coord? Point);

public sealed record WithCoordAttr([property: Avm1Converter(typeof(CoordConverter))] Coord Position);

public sealed record WithVector(Vector V);

public sealed record Holder([property: Avm1Converter(typeof(TaggedPropConverter))] Tagged T);

public sealed record Holder2(Tagged T);

public sealed record Nested(string Name, Coord Point, List<int> Values, int[][] Grid, Dictionary<string, int> Map);

public sealed record Default(int A);

public sealed class Keyed
{
    [Avm1Property("n")]
    public string Name { get; set; } = "";

    [Avm1Ignore]
    public int Skip { get; set; }

    public int Keep { get; set; }
}

public sealed class Settings
{
    public string? Theme { get; init; }

    public bool Mute { get; init; }
}
