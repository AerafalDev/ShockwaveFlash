// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Control;

[Flags]
public enum FileAttributesFlags : uint
{
    UseDirectBlit = 1 << 6,
    UseGpu = 1 << 5,
    HasMetadata = 1 << 4,
    IsActionScript3 = 1 << 3,
    UseNetworkSandbox = 1 << 0
}
