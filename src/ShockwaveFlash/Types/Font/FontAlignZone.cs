// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Font;

public sealed record FontAlignZone(short Left, short Width, short Bottom, short Height)
{
    public static FontAlignZone Decode(ref SpanReader reader)
    {
        reader.Advance(sizeof(byte));
        var zone = new FontAlignZone(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
        reader.Advance(sizeof(byte));
        return zone;
    }
}
