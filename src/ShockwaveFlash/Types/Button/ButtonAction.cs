// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.


namespace ShockwaveFlash.Types.Button;

public sealed record ButtonAction(ButtonActionCondition Conditions, ReadOnlyMemory<byte> Data)
{
    public static (ButtonAction, bool) Decode(MemoryReader reader)
    {
        var length = reader.ReadUInt16();
        var conditions = (ButtonActionCondition)reader.ReadUInt16();

        var data = length switch
        {
            >= 4 => reader.ReadMemory(length - 4),
            0 => reader.ReadMemoryToEnd(),
            _ => throw new NotSupportedException("Button actions length is too short.")
        };

        return (new ButtonAction(conditions, data), length is not 0);
    }
}
