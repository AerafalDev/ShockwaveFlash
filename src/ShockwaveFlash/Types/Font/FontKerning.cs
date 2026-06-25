namespace ShockwaveFlash.Types.Font;

public sealed record FontKerning(ushort LeftCode, ushort RightCode, short Adjustment)
{
    public static FontKerning Decode(MemoryReader reader, bool wideCodes)
    {
        var leftCode = wideCodes ? reader.ReadUInt16() : reader.ReadUInt8();
        var rightCode = wideCodes ? reader.ReadUInt16() : reader.ReadUInt8();
        var adjustment = reader.ReadInt16();

        return new FontKerning(leftCode, rightCode, adjustment);
    }

    public void Encode(MemoryWriter writer, bool wideCodes)
    {
        if (wideCodes)
            writer.WriteUInt16(LeftCode);
        else
            writer.WriteUInt8((byte)LeftCode);

        if (wideCodes)
            writer.WriteUInt16(RightCode);
        else
            writer.WriteUInt8((byte)RightCode);

        writer.WriteInt16(Adjustment);
    }
}
