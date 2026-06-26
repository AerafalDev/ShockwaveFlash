namespace ShockwaveFlash.Types.Text;

public sealed class TextLayout
{
    public TextAlignment Alignment { get; set; }

    public ushort LeftMargin { get; set; }

    public ushort RightMargin { get; set; }

    public short Indent { get; set; }

    public short Leading { get; set; }

    public TextLayout(TextAlignment alignment, ushort leftMargin, ushort rightMargin, short indent, short leading)
    {
        Alignment = alignment;
        LeftMargin = leftMargin;
        RightMargin = rightMargin;
        Indent = indent;
        Leading = leading;
    }

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
