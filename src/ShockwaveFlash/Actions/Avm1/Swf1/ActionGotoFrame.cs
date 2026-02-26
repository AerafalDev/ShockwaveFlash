// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Actions.Avm1.Swf1;

public sealed record ActionGotoFrame(ushort Frame) : Action(ActionOpcode.GotoFrame)
{
    public static ActionGotoFrame Decode(ref SpanReader reader)
    {
        return new ActionGotoFrame(reader.ReadUInt16());
    }
}
