// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Font;

public sealed record FontAlignZone(short Left, short Width, short Bottom, short Height)
{
    public static FontAlignZone Decode(MemoryReader reader)
    {
        reader.Advance(sizeof(byte));
        var zone = new FontAlignZone(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
        reader.Advance(sizeof(byte));
        return zone;
    }

    public void Encode(MemoryWriter writer)
    {
        writer.WriteUInt8(2);
        writer.WriteInt16(Left);
        writer.WriteInt16(Width);
        writer.WriteInt16(Bottom);
        writer.WriteInt16(Height);
        writer.WriteUInt8(0);
    }
}
