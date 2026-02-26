// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Actions.Types;

[Flags]
public enum GetUrlFlags : byte
{
    MethodNone = 0,
    MethodGet = 1,
    MethodPost = 2,
    MethodMask = 3,

    LoadTarget = 1 << 6,
    LoadVariables = 1 << 7
}
