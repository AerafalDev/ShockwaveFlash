using ShockwaveFlash.Exceptions;

namespace ShockwaveFlash.Types.Button;

public sealed class ButtonAction
{
    public ButtonActionCondition Conditions { get; set; }

    public ReadOnlyMemory<byte> Data { get; set; }

    public ButtonAction(ButtonActionCondition conditions, ReadOnlyMemory<byte> data)
    {
        Conditions = conditions;
        Data = data;
    }

    public static (ButtonAction, bool) Decode(MemoryReader reader)
    {
        var length = reader.ReadUInt16();
        var conditions = (ButtonActionCondition)reader.ReadUInt16();

        var data = length switch
        {
            >= 4 => reader.ReadMemory(length - 4),
            0 => reader.ReadMemoryToEnd(),
            _ => throw new SwfFormatException("Button action length is too short.")
        };

        return (new ButtonAction(conditions, data), length is not 0);
    }

    public static void EncodeCollection(MemoryWriter writer, IReadOnlyList<ButtonAction> actions)
    {
        for (var i = 0; i < actions.Count; i++)
            actions[i].Encode(writer, i == actions.Count - 1);
    }

    public void Encode(MemoryWriter writer, bool isLast)
    {
        writer.WriteUInt16(isLast ? (ushort)0 : (ushort)(Data.Length + 4));
        writer.WriteUInt16((ushort)Conditions);
        writer.WriteMemory(Data);
    }
}
