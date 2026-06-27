using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Swf7;

public sealed record ActionTry(TryFlags Flags, byte CatchRegister, string CatchVariable, ReadOnlyMemory<byte> TryBody, ReadOnlyMemory<byte> CatchBody, ReadOnlyMemory<byte> FinallyBody) : Action(ActionOpcode.Try)
{
    public static ActionTry Decode(MemoryReader header, MemoryReader outer, Avm1Context context)
    {
        var flags = (TryFlags)header.ReadUInt8();
        var trySize = header.ReadUInt16();
        var catchSize = header.ReadUInt16();
        var finallySize = header.ReadUInt16();

        var (catchRegister, catchVariable) = flags.HasFlag(TryFlags.CatchInRegister)
            ? (header.ReadUInt8(), string.Empty)
            : ((byte)0, header.ReadNullTerminatedString(context.Encoding));

        var tryBody = outer.ReadMemory(trySize);
        var catchBody = outer.ReadMemory(catchSize);
        var finallyBody = outer.ReadMemory(finallySize);

        return new ActionTry(flags, catchRegister, catchVariable, tryBody, catchBody, finallyBody);
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteUInt8((byte)Flags);
        writer.WriteUInt16((ushort)TryBody.Length);
        writer.WriteUInt16((ushort)CatchBody.Length);
        writer.WriteUInt16((ushort)FinallyBody.Length);
        if (Flags.HasFlag(TryFlags.CatchInRegister))
            writer.WriteUInt8(CatchRegister);
        else
            writer.WriteNullTerminatedString(CatchVariable, context.Encoding);
    }

    public override void EncodeTrailer(MemoryWriter writer)
    {
        writer.WriteMemory(TryBody);
        writer.WriteMemory(CatchBody);
        writer.WriteMemory(FinallyBody);
    }
}
