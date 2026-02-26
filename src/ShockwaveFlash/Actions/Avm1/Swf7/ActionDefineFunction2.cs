// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

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
}
