using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Swf7;

public sealed class ActionDefineFunction2 : Action
{
    public string Name { get; set; }

    public byte RegisterCount { get; set; }

    public FunctionFlags Flags { get; set; }

    public IReadOnlyList<FunctionParameter> Parameters { get; set; }

    public ReadOnlyMemory<byte> Body { get; set; }

    public bool PreloadThis =>
        Flags.HasFlag(FunctionFlags.PreloadThis);

    public bool SuppressThis =>
        Flags.HasFlag(FunctionFlags.SuppressThis);

    public bool PreloadArguments =>
        Flags.HasFlag(FunctionFlags.PreloadArguments);

    public bool SuppressArguments =>
        Flags.HasFlag(FunctionFlags.SuppressArguments);

    public bool PreloadSuper =>
        Flags.HasFlag(FunctionFlags.PreloadSuper);

    public bool SuppressSuper =>
        Flags.HasFlag(FunctionFlags.SuppressSuper);

    public bool PreloadRoot =>
        Flags.HasFlag(FunctionFlags.PreloadRoot);

    public bool PreloadParent =>
        Flags.HasFlag(FunctionFlags.PreloadParent);

    public bool PreloadGlobal =>
        Flags.HasFlag(FunctionFlags.PreloadGlobal);

    public ActionDefineFunction2(string name, byte registerCount, FunctionFlags flags, IReadOnlyList<FunctionParameter> parameters, ReadOnlyMemory<byte> body) : base(ActionOpcode.DefineFunction2)
    {
        Name = name;
        RegisterCount = registerCount;
        Flags = flags;
        Parameters = parameters;
        Body = body;
    }

    public static ActionDefineFunction2 Decode(MemoryReader header, MemoryReader outer, Avm1Context context)
    {
        var name = header.ReadNullTerminatedString(context.Encoding);
        var numParams = header.ReadUInt16();
        var registerCount = header.ReadUInt8();
        var flags = (FunctionFlags)header.ReadUInt16();

        var parameters = new FunctionParameter[numParams];

        for (var i = 0; i < numParams; i++)
            parameters[i] = new FunctionParameter(header.ReadUInt8(), header.ReadNullTerminatedString(context.Encoding));

        var codeSize = header.ReadUInt16();

        return new ActionDefineFunction2(name, registerCount, flags, parameters, outer.ReadMemory(codeSize));
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteNullTerminatedString(Name, context.Encoding);
        writer.WriteUInt16((ushort)Parameters.Count);
        writer.WriteUInt8(RegisterCount);
        writer.WriteUInt16((ushort)Flags);
        foreach (var parameter in Parameters)
        {
            writer.WriteUInt8(parameter.Register);
            writer.WriteNullTerminatedString(parameter.Name, context.Encoding);
        }

        writer.WriteUInt16(CheckedBodySize(Body.Length));
    }

    public override void EncodeTrailer(MemoryWriter writer)
    {
        writer.WriteMemory(Body);
    }
}
