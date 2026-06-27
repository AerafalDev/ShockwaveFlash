namespace ShockwaveFlash.Avm1.Swf4;

public sealed class ActionGotoFrame2 : Action
{
    public bool Play { get; set; }

    public bool HasSceneBias { get; set; }

    public ushort SceneBias { get; set; }

    public ActionGotoFrame2(bool play, bool hasSceneBias, ushort sceneBias) : base(ActionOpcode.GotoFrame2)
    {
        Play = play;
        HasSceneBias = hasSceneBias;
        SceneBias = sceneBias;
    }

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
