using ShockwaveFlash.Types.Font;

namespace ShockwaveFlash.Tags.Font;

public sealed class DefineFontInfoTag : Tag
{
    public ushort Id { get; set; }

    public string Name { get; set; }

    public FontInfoFlags Flags { get; set; }

    public IReadOnlyList<ushort> CodeTable { get; set; }

    public bool HasWideCodes =>
        Flags.HasFlag(FontInfoFlags.HasWideCodes);

    public bool IsBold =>
        Flags.HasFlag(FontInfoFlags.IsBold);

    public bool IsItalic =>
        Flags.HasFlag(FontInfoFlags.IsItalic);

    public bool IsShiftJis =>
        Flags.HasFlag(FontInfoFlags.IsShiftJis);

    public bool IsAnsi =>
        Flags.HasFlag(FontInfoFlags.IsAnsi);

    public bool IsSmallText =>
        Flags.HasFlag(FontInfoFlags.IsSmallText);

    public DefineFontInfoTag(TagMetadata metadata, ushort id, string name, FontInfoFlags flags, IReadOnlyList<ushort> codeTable) : base(metadata)
    {
        Id = id;
        Name = name;
        Flags = flags;
        CodeTable = codeTable;
    }

    public static DefineFontInfoTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var name = reader.ReadLengthPrefixedString();
        var flags = (FontInfoFlags)reader.ReadUInt8();
        var codeTable = new List<ushort>();

        while (reader.Remaining > 0)
            codeTable.Add(flags.HasFlag(FontInfoFlags.HasWideCodes) ? reader.ReadUInt16() : reader.ReadUInt8());

        return new DefineFontInfoTag(metadata, id, name, flags, codeTable);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteLengthPrefixedString(Name);
        writer.WriteUInt8((byte)Flags);

        foreach (var code in CodeTable)
        {
            if (HasWideCodes)
                writer.WriteUInt16(code);
            else
                writer.WriteUInt8((byte)code);
        }
    }
}
