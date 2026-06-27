using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Swf4;

public sealed class ActionGetURL2 : Action
{
    public GetUrlFlags Flags { get; set; }

    public bool MethodNone =>
        Flags.HasFlag(GetUrlFlags.MethodNone);

    public bool MethodGet =>
        Flags.HasFlag(GetUrlFlags.MethodGet);

    public bool MethodPost =>
        Flags.HasFlag(GetUrlFlags.MethodPost);

    public bool LoadTarget =>
        Flags.HasFlag(GetUrlFlags.LoadTarget);

    public bool LoadVariables =>
        Flags.HasFlag(GetUrlFlags.LoadVariables);

    public SendVarsMethod SendVarsMethod =>
        (SendVarsMethod)(byte)(Flags & GetUrlFlags.MethodMask);

    public ActionGetURL2(GetUrlFlags flags) : base(ActionOpcode.GetURL2)
    {
        Flags = flags;
    }

    public static ActionGetURL2 Decode(MemoryReader reader)
    {
        return new ActionGetURL2((GetUrlFlags)reader.ReadUInt8());
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteUInt8((byte)Flags);
    }
}
