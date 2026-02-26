// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Actions.Types;

[Flags]
public enum TryFlags : byte
{
    CatchBlock = 1 << 0,
    FinallyBlock = 1 << 1,
    CatchInRegister = 1 << 2
}
