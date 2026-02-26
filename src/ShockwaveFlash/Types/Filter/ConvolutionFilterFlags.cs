// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Filter;

[Flags]
public enum ConvolutionFilterFlags : byte
{
    Clamp = 1 << 1,
    PreserveAlpha = 1 << 0
}
