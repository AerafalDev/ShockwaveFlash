// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Text;

public sealed record TextLayout(TextAlignment Alignment, ushort LeftMargin, ushort RightMargin, short Indent, short Leading)
{
    public static TextLayout Decode(ref SpanReader reader)
    {
        var alignment = (TextAlignment)reader.ReadUInt8();
        var leftMargin = reader.ReadUInt16();
        var rightMargin = reader.ReadUInt16();
        var indent = reader.ReadInt16();
        var leading = reader.ReadInt16();

        return new TextLayout(alignment, leftMargin, rightMargin, indent, leading);
    }
}
