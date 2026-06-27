namespace ShockwaveFlash.Avm1.Swf5;

public sealed record ActionWith(ReadOnlyMemory<byte> Body) : Action(ActionOpcode.With)
{
    public static ActionWith Decode(MemoryReader header, MemoryReader outer)
    {
        var codeSize = header.ReadUInt16();

        return new ActionWith(outer.ReadMemory(codeSize));
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteUInt16((ushort)Body.Length);
    }

    public override void EncodeTrailer(MemoryWriter writer)
    {
        writer.WriteMemory(Body);
    }
}
