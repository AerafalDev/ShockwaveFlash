using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Swf7;

public sealed class ActionTry : Action
{
    public TryFlags Flags { get; set; }

    public byte CatchRegister { get; set; }

    public string CatchVariable { get; set; }

    public ReadOnlyMemory<byte> TryBody { get; set; }

    public ReadOnlyMemory<byte> CatchBody { get; set; }

    public ReadOnlyMemory<byte> FinallyBody { get; set; }

    public ActionTry(TryFlags flags, byte catchRegister, string catchVariable, ReadOnlyMemory<byte> tryBody, ReadOnlyMemory<byte> catchBody, ReadOnlyMemory<byte> finallyBody) : base(ActionOpcode.Try)
    {
        Flags = flags;
        CatchRegister = catchRegister;
        CatchVariable = catchVariable;
        TryBody = tryBody;
        CatchBody = catchBody;
        FinallyBody = finallyBody;
    }

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
        writer.WriteUInt16(CheckedBodySize(TryBody.Length));
        writer.WriteUInt16(CheckedBodySize(CatchBody.Length));
        writer.WriteUInt16(CheckedBodySize(FinallyBody.Length));
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
