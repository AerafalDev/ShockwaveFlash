// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Actions.Avm1.Special;

public sealed record ActionUnknown(ActionOpcode Opcode, SpanSlice Data) : Action(Opcode)
{
    public static ActionUnknown Decode(ref SpanReader reader, ActionOpcode opcode)
    {
        return new ActionUnknown(opcode, reader.SliceToEnd());
    }
}
