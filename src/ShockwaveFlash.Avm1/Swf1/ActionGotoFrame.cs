namespace ShockwaveFlash.Avm1.Swf1;

public sealed record ActionGotoFrame(ushort Frame) : Action(ActionOpcode.GotoFrame)
{
    public static ActionGotoFrame Decode(MemoryReader reader)
    {
        return new ActionGotoFrame(reader.ReadUInt16());
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteUInt16(Frame);
    }
}
