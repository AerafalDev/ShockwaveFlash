// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Shape;

[Flags]
public enum ShapeRecordFlags : byte
{
    None = 0,
    MoveTo = 1 << 0,
    FillStyle0 = 1 << 1,
    FillStyle1 = 1 << 2,
    LineStyle = 1 << 3,
    NewStyles = 1 << 4
}
