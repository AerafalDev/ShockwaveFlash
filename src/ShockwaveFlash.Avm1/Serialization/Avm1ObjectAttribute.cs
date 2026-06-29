namespace ShockwaveFlash.Avm1.Serialization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class Avm1ObjectAttribute : Attribute
{
    public string? GlobalName { get; }

    public Avm1ObjectAttribute(string? globalName = null)
    {
        GlobalName = globalName;
    }
}
