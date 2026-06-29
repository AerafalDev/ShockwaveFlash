using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization;

public interface IAvm1Serializable<TSelf>
    where TSelf : IAvm1Serializable<TSelf>
{
    static abstract string? Avm1GlobalName { get; }

    Avm1Object ToAvm1Object();

    static abstract TSelf FromAvm1Object(Avm1Object source);
}
