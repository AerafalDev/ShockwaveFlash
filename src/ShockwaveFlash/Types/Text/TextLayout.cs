namespace ShockwaveFlash.Types.Text;

public sealed record TextLayout(TextAlignment Alignment, ushort LeftMargin, ushort RightMargin, short Indent, short Leading)
{
    public static TextLayout Decode(MemoryReader reader)
    {
        var alignment = (TextAlignment)reader.ReadUInt8();
        var leftMargin = reader.ReadUInt16();
        var rightMargin = reader.ReadUInt16();
        var indent = reader.ReadInt16();
        var leading = reader.ReadInt16();

        return new TextLayout(alignment, leftMargin, rightMargin, indent, leading);
    }

    public void Encode(MemoryWriter writer)
    {
        writer.WriteUInt8((byte)Alignment);
        writer.WriteUInt16(LeftMargin);
        writer.WriteUInt16(RightMargin);
        writer.WriteInt16(Indent);
        writer.WriteInt16(Leading);
    }
}
