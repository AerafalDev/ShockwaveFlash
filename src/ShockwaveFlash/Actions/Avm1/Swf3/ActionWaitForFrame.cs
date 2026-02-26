// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Actions.Avm1.Swf3;

public sealed record ActionWaitForFrame(ushort Frame, byte SkipCount) : Action(ActionOpcode.WaitForFrame)
{
    public static ActionWaitForFrame Decode(ref SpanReader reader)
    {
        return new ActionWaitForFrame(reader.ReadUInt16(), reader.ReadUInt8());
    }
}
