namespace ShockwaveFlash.Avm1.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class Avm1PropertyOrderAttribute : Attribute
{
    public Avm1PropertyOrderAttribute(int order)
    {
        Order = order;
    }

    public int Order { get; }
}
