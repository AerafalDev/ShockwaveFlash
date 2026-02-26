// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Actions.Avm1.Swf4;

public sealed record ActionIf(short BranchOffset) : Action(ActionOpcode.If)
{
    public static ActionIf Decode(ref SpanReader reader)
    {
        return new ActionIf(reader.ReadInt16());
    }
}
