// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Actions.Types;

namespace ShockwaveFlash.Actions.Avm1.Swf4;

public sealed record ActionGetURL2(GetUrlFlags Flags) : Action(ActionOpcode.GetURL2)
{
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

    public static ActionGetURL2 Decode(ref SpanReader reader)
    {
        return new ActionGetURL2((GetUrlFlags)reader.ReadUInt8());
    }
}
