// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Morph;

[Flags]
public enum DefineMorphShapeFlags : byte
{
    HasScalingStrokes = 1 << 0,
    HasNonScalingStrokes = 1 << 1
}
