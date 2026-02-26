// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;

namespace ShockwaveFlash.Actions.Avm1.Swf3;

public sealed record ActionSetTarget(string TargetName) : Action(ActionOpcode.SetTarget)
{
    public static ActionSetTarget Decode(ref SpanReader reader, Encoding encoding)
    {
        return new ActionSetTarget(reader.ReadNullTerminatedString(encoding));
    }
}
