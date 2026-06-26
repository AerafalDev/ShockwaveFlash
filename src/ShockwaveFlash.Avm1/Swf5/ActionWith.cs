namespace ShockwaveFlash.Avm1.Swf5;

public sealed record ActionWith(int CodeSize) : Action(ActionOpcode.With)
{
    public static ActionWith Decode(MemoryReader reader)
    {
        return new ActionWith(reader.ReadUInt16());
    }
}
