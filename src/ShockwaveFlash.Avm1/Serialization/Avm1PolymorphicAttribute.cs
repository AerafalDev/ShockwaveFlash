namespace ShockwaveFlash.Avm1.Serialization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class Avm1PolymorphicAttribute : Attribute
{
    public string TypeDiscriminatorPropertyName { get; set; } = "$type";
}
