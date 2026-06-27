namespace ShockwaveFlash.Avm1.Swf3;

public sealed class ActionWaitForFrame : Action
{
    public ushort Frame { get; set; }

    public byte SkipCount { get; set; }

    public ActionWaitForFrame(ushort frame, byte skipCount) : base(ActionOpcode.WaitForFrame)
    {
        Frame = frame;
        SkipCount = skipCount;
    }

    public static ActionWaitForFrame Decode(MemoryReader reader)
    {
        return new ActionWaitForFrame(reader.ReadUInt16(), reader.ReadUInt8());
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteUInt16(Frame);
        writer.WriteUInt8(SkipCount);
    }
}
