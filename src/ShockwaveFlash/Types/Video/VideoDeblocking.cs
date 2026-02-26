// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Video;

public enum VideoDeblocking : byte
{
    UseVideoPacketValue = 0,
    None = 1,
    Level1 = 2,
    Level2 = 3,
    Level3 = 4,
    Level4 = 5
}
