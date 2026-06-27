namespace ShockwaveFlash.Avm1.Swf5;

public sealed record ActionWith(int CodeSize) : Action(ActionOpcode.With)
{
    public static ActionWith Decode(MemoryReader reader)
    {
        return new ActionWith(reader.ReadUInt16());
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteUInt16((ushort)CodeSize);
    }
}
