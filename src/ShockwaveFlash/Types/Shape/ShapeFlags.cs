// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Shape;

[Flags]
public enum ShapeFlags : byte
{
    HasScalingStrokes = 1 << 0,
    HasNonScalingStrokes = 1 << 1,
    NonZeroWindingRule = 1 << 2
}
