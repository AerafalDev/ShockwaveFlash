using System.Collections.Generic;

namespace ShockwaveFlash.Avm1.Serialization.Metadata;

public abstract class Avm1TypeInfo
{
    private readonly Lock _gate = new();
    private Action<Avm1TypeInfo>? _populate;
    private bool _populated;

    public Type Type { get; }

    public Avm1TypeInfoKind Kind { get; set; }

    public IList<Avm1PropertyInfo> Properties { get; } = [];

    public string[]? BindingPath { get; set; }

    internal Avm1SerializerOptions? Options { get; set; }

    internal Avm1Converter Converter { get; set; } = null!;

    internal Func<object?[], object>? ConstructorFactory { get; set; }

    internal Func<object>? ObjectFactory { get; set; }

    internal string[] ConstructorArguments { get; set; } = [];

    private protected Avm1TypeInfo(Type type)
    {
        Type = type;
    }

    internal void SetPopulate(Action<Avm1TypeInfo> populate)
    {
        _populate = populate;
    }

    internal void EnsurePopulated()
    {
        if (_populated)
            return;

        lock (_gate)
        {
            if (_populated)
                return;

            _populate?.Invoke(this);
            _populated = true;
        }
    }
}
