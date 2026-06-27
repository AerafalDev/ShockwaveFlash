namespace ShockwaveFlash.Avm1.Swf4;

public sealed record ActionGotoFrame2(bool Play, bool HasSceneBias, ushort SceneBias) : Action(ActionOpcode.GotoFrame2)
{
    public static ActionGotoFrame2 Decode(MemoryReader reader)
    {
        var flags = reader.ReadUInt8();
        var play = (flags & 1) is not 0;
        var hasSceneBias = (flags & 2) is not 0;
        var sceneBias = hasSceneBias ? reader.ReadUInt16() : (ushort)0;

        return new ActionGotoFrame2(play, hasSceneBias, sceneBias);
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteUInt8((byte)((Play ? 1 : 0) | (HasSceneBias ? 2 : 0)));
        if (HasSceneBias)
            writer.WriteUInt16(SceneBias);
    }
}
