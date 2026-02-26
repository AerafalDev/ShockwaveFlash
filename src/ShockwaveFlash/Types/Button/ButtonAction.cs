// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Types.Button;

public sealed record ButtonAction(ButtonActionCondition Conditions, SpanSlice Data)
{
    public static (ButtonAction, bool) Decode(ref SpanReader reader)
    {
        var length = reader.ReadUInt16();
        var conditions = (ButtonActionCondition)reader.ReadUInt16();

        var data = length switch
        {
            >= 4 => reader.Slice(length - 4),
            0 => reader.SliceToEnd(),
            _ => throw new NotSupportedException("Button actions length is too short.")
        };

        return (new ButtonAction(conditions, data), length is not 0);
    }
}
