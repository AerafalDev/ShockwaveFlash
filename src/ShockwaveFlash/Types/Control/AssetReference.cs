// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Control;

public sealed record AssetReference(ushort Id, string Name)
{
    public static AssetReference Decode(ref SpanReader reader)
    {
        return new AssetReference(reader.ReadUInt16(), reader.ReadNullTerminatedString());
    }
}
