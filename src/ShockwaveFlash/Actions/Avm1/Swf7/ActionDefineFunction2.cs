// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using ShockwaveFlash.Actions.Types;

namespace ShockwaveFlash.Actions.Avm1.Swf7;

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

    public static ActionDefineFunction2 Decode(ref SpanReader reader, Encoding encoding)
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
}
