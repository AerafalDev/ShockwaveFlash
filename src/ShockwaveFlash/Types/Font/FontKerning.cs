// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Font;

public sealed record FontKerning(ushort LeftCode, ushort RightCode, short Adjustment)
{
    public static FontKerning Decode(ref SpanReader reader, bool wideCodes)
    {
        var leftCode = wideCodes ? reader.ReadUInt16() : reader.ReadUInt8();
        var rightCode = wideCodes ? reader.ReadUInt16() : reader.ReadUInt8();
        var adjustment = reader.ReadInt16();

        return new FontKerning(leftCode, rightCode, adjustment);
    }
}
