// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.


namespace ShockwaveFlash.Types.DisplayList;

public sealed record ClipAction(ClipEventFlags Events, KeyCode? KeyCode, ReadOnlyMemory<byte> Data)
{
    public static IReadOnlyList<ClipAction> DecodeCollection(MemoryReader reader, byte swfVersion)
    {
        _ = reader.ReadUInt16();
        _ = ClipEventFlags.Decode(reader, swfVersion);

        var clipActions = new List<ClipAction>();

        var clipAction = Decode(reader, swfVersion);

        while (clipAction is not null)
        {
            clipActions.Add(clipAction);
            clipAction = Decode(reader, swfVersion);
        }

        return clipActions;
    }

    private static ClipAction? Decode(MemoryReader reader, byte swfVersion)
    {
        var events = ClipEventFlags.Decode(reader, swfVersion);

        if (events is ClipEventFlags.None)
            return null;

        var length = reader.ReadInt32();

        KeyCode? keyCode = null;

        if (events.HasFlag(ClipEventFlags.KeyPress))
        {
            length--;
            keyCode = (KeyCode)reader.ReadUInt8();
        }

        var data = reader.ReadMemory(length);

        return new ClipAction(events, keyCode, data);
    }
}
