namespace ShockwaveFlash.Types.Control;

public sealed class SymbolReference
{
    public ushort Id { get; set; }

    public string Name { get; set; }

    public SymbolReference(ushort id, string name)
    {
        Id = id;
        Name = name;
    }
}
