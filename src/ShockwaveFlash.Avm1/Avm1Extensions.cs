using ShockwaveFlash.Avm1.Types;
using ShockwaveFlash.Tags.Action;

namespace ShockwaveFlash.Avm1;

public static class Avm1Extensions
{
    public static IReadOnlyList<Action> DecodeActions(this DoActionTag tag, byte swfVersion)
    {
        return Action.DecodeCollection(tag.Data, swfVersion);
    }

    public static Avm1Object Evaluate(this DoActionTag tag, byte swfVersion, bool strict = false)
    {
        return Avm1Machine.Run(tag.Data, swfVersion, strict);
    }

    public static DoActionTag WithActions(this DoActionTag tag, IReadOnlyList<Action> actions, byte swfVersion)
    {
        return new DoActionTag(tag.Metadata, Action.EncodeCollection(actions, swfVersion));
    }

    public static DoActionTag WithGlobals(this DoActionTag tag, Avm1Object globals, byte swfVersion)
    {
        return new DoActionTag(tag.Metadata, Avm1Emitter.EmitBytes(globals, swfVersion));
    }
}
