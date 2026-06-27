namespace ShockwaveFlash.Avm1.Swf1;

public sealed class ActionGotoFrame : Action
{
    public ushort Frame { get; set; }

    public ActionGotoFrame(ushort frame) : base(ActionOpcode.GotoFrame)
    {
        Frame = frame;
    }

    public static ActionGotoFrame Decode(MemoryReader reader)
    {
        return new ActionGotoFrame(reader.ReadUInt16());
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteUInt16(Frame);
    }
}
