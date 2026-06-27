using System.Text;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Swf7;

public sealed record ActionDefineFunction2(string Name, byte RegisterCount, FunctionFlags Flags, IReadOnlyList<FunctionParameter> Parameters, int CodeSize)
    : Action(ActionOpcode.DefineFunction2)
{
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

    public static ActionDefineFunction2 Decode(MemoryReader reader, Encoding encoding)
    {
        var name = reader.ReadNullTerminatedString(encoding);
        var numParams = reader.ReadUInt16();
        var registerCount = reader.ReadUInt8();
        var flags = (FunctionFlags)reader.ReadUInt16();

        var parameters = new FunctionParameter[numParams];

        for (var i = 0; i < numParams; i++)
            parameters[i] = new FunctionParameter(reader.ReadUInt8(), reader.ReadNullTerminatedString(encoding));

        var codeSize = reader.ReadUInt16();

        return new ActionDefineFunction2(name, registerCount, flags, parameters, codeSize);
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

        writer.WriteUInt16((ushort)CodeSize);
    }
}
