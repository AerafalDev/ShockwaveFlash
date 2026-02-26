// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Actions.Types;

public enum PushValueType : byte
{
    String = 0,
    Float = 1,
    Null = 2,
    Undefined = 3,
    Register = 4,
    Boolean = 5,
    Double = 6,
    Integer = 7,
    Constant8 = 8,
    Constant16 = 9
}
