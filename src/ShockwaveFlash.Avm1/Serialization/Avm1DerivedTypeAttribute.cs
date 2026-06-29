namespace ShockwaveFlash.Avm1.Serialization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
public sealed class Avm1DerivedTypeAttribute : Attribute
{
    public Avm1DerivedTypeAttribute(Type derivedType, string typeDiscriminator)
    {
        DerivedType = derivedType;
        TypeDiscriminator = typeDiscriminator;
    }

    public Type DerivedType { get; }

    public string TypeDiscriminator { get; }
}
