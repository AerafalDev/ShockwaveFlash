
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

    public static void EncodeCollection(MemoryWriter writer, IReadOnlyList<ClipAction> clipActions, byte swfVersion)
    {
        var allEvents = ClipEventFlags.None;

        foreach (var clipAction in clipActions)
            allEvents |= clipAction.Events;

        writer.WriteUInt16(0);
        allEvents.Encode(writer, swfVersion);

        foreach (var clipAction in clipActions)
            clipAction.Encode(writer, swfVersion);

        ClipEventFlags.None.Encode(writer, swfVersion);
    }

    public void Encode(MemoryWriter writer, byte swfVersion)
    {
        Events.Encode(writer, swfVersion);

        var hasKeyPress = Events.HasFlag(ClipEventFlags.KeyPress);
        var length = Data.Length + (hasKeyPress ? 1 : 0);

        writer.WriteInt32(length);

        if (hasKeyPress && KeyCode is { } keyCode)
            writer.WriteUInt8((byte)keyCode);

        writer.WriteMemory(Data);
    }
}
