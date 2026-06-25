// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Control;

public sealed record AssetReference(ushort Id, string Name)
{
    public static AssetReference Decode(MemoryReader reader)
    {
        return new AssetReference(reader.ReadUInt16(), reader.ReadNullTerminatedString());
    }

    public void Encode(MemoryWriter writer)
    {
        writer.WriteUInt16(Id);
        writer.WriteNullTerminatedString(Name);
    }
}
