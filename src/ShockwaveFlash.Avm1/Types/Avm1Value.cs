namespace ShockwaveFlash.Avm1.Types;

public abstract record Avm1Value
{
    public static Avm1Value Null { get; } = new Avm1Null();

    public static Avm1Value Undefined { get; } = new Avm1Undefined();

    public Avm1Object AsObject => (Avm1Object)this;

    public Avm1Array AsArray => (Avm1Array)this;

    public string AsString => ((Avm1String)this).Value;

    public double AsNumber => ((Avm1Number)this).Value;

    public bool AsBoolean => ((Avm1Boolean)this).Value;

    public bool IsNull => this is Avm1Null;

    public bool IsUndefined => this is Avm1Undefined;

    public static implicit operator Avm1Value(string value) => new Avm1String(value);

    public static implicit operator Avm1Value(double value) => new Avm1Number(value);

    public static implicit operator Avm1Value(int value) => new Avm1Number(value);

    public static implicit operator Avm1Value(bool value) => new Avm1Boolean(value);

    public static Avm1Value From(string value) => new Avm1String(value);

    public static Avm1Value From(double value) => new Avm1Number(value);

    public static Avm1Value From(bool value) => new Avm1Boolean(value);
}

public sealed record Avm1String(string Value) : Avm1Value;

public sealed record Avm1Number(double Value) : Avm1Value;

public sealed record Avm1Boolean(bool Value) : Avm1Value;

public sealed record Avm1Null : Avm1Value;

public sealed record Avm1Undefined : Avm1Value;

public sealed record Avm1Object : Avm1Value
{
    public Dictionary<string, Avm1Value> Members { get; init; } = new(StringComparer.Ordinal);

    public Avm1Value this[string key]
    {
        get => Members[key];
        set => Members[key] = value;
    }

    public override string ToString() => "[object Object]";
}

public sealed record Avm1Array : Avm1Value
{
    public List<Avm1Value> Items { get; init; } = [];

    public Avm1Value this[int index]
    {
        get => Items[index];
        set => Items[index] = value;
    }

    public override string ToString() => "[object Array]";
}
