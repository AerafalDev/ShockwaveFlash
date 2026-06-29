namespace ShockwaveFlash.Avm1.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class Avm1PropertyAttribute : Attribute
{
    public string Key { get; }

    public Avm1PropertyAttribute(string key)
    {
        Key = key;
    }
}
