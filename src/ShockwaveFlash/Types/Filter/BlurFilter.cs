// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;

namespace ShockwaveFlash.Types.Filter;

public sealed record BlurFilter(Vector2 Blur, BlurFilterFlags Flags) : Filter
{
    public byte Passes =>
        (byte)((byte)(Flags & BlurFilterFlags.Passes) >> 3);

    public bool Impotent =>
        Passes is 0 || Blur is { X: <= 1 << 16, Y: <= 1 << 16 };

    public static new BlurFilter Decode(ref SpanReader reader)
    {
        var blur = reader.ReadVector2();
        var flags = (BlurFilterFlags)reader.ReadUInt8();

        return new BlurFilter(blur, flags);
    }
}
