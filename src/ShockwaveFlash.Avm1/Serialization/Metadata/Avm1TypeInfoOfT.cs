namespace ShockwaveFlash.Avm1.Serialization.Metadata;

public sealed class Avm1TypeInfo<T> : Avm1TypeInfo
{
    public Avm1TypeInfo() : base(typeof(T))
    {
    }
}
