// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Types.DisplayList;

public sealed record ClipAction(ClipEventFlags Events, KeyCode? KeyCode, SpanSlice Data)
{
    public static IReadOnlyList<ClipAction> DecodeCollection(ref SpanReader reader, byte swfVersion)
    {
        _ = reader.ReadUInt16();
        _ = ClipEventFlags.Decode(ref reader, swfVersion);

        var clipActions = new List<ClipAction>();

        var clipAction = Decode(ref reader, swfVersion);

        while (clipAction is not null)
        {
            clipActions.Add(clipAction);
            clipAction = Decode(ref reader, swfVersion);
        }

        return clipActions;
    }

    private static ClipAction? Decode(ref SpanReader reader, byte swfVersion)
    {
        var events = ClipEventFlags.Decode(ref reader, swfVersion);

        if (events is ClipEventFlags.None)
            return null;

        var length = reader.ReadInt32();

        KeyCode? keyCode = null;

        if (events.HasFlag(ClipEventFlags.KeyPress))
        {
            length--;
            keyCode = (KeyCode)reader.ReadUInt8();
        }

        var data = reader.Slice(length);

        return new ClipAction(events, keyCode, data);
    }
}
