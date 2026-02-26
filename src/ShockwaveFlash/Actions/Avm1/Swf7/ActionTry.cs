// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using ShockwaveFlash.Actions.Types;

namespace ShockwaveFlash.Actions.Avm1.Swf7;

public sealed record ActionTry(TryFlags Flags, byte CatchRegister, string CatchVariable, ushort TrySize, ushort CatchSize, ushort FinallySize) : Action(ActionOpcode.Try)
{
    public static ActionTry Decode(ref SpanReader reader, Encoding encoding)
    {
        var flags = (TryFlags)reader.ReadUInt8();
        var trySize = reader.ReadUInt16();
        var catchSize = reader.ReadUInt16();
        var finallySize = reader.ReadUInt16();

        var (catchRegister, catchVariable) = flags.HasFlag(TryFlags.CatchInRegister)
            ? (reader.ReadUInt8(), string.Empty)
            : ((byte)0, reader.ReadNullTerminatedString(encoding));

        return new ActionTry(flags, catchRegister, catchVariable, trySize, catchSize, finallySize);
    }
}
