// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Button;

[Flags]
public enum ButtonStates : byte
{
    Up = 1 << 0,
    Over = 1 << 1,
    Down = 1 << 2,
    HitTest = 1 << 3
}
