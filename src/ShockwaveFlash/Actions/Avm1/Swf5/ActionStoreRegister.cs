// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Actions.Avm1.Swf5;

public sealed record ActionStoreRegister(byte RegisterNumber) : Action(ActionOpcode.StoreRegister)
{
    public static ActionStoreRegister Decode(ref SpanReader reader)
    {
        return new ActionStoreRegister(reader.ReadUInt8());
    }
}
