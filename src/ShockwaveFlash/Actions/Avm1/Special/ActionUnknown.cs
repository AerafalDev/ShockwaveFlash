// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.


namespace ShockwaveFlash.Actions.Avm1.Special;

public sealed record ActionUnknown(ActionOpcode Opcode, ReadOnlyMemory<byte> Data) : Action(Opcode)
{
    public static ActionUnknown Decode(MemoryReader reader, ActionOpcode opcode)
    {
        return new ActionUnknown(opcode, reader.ReadMemoryToEnd());
    }
}
