using System.Diagnostics.CodeAnalysis;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Text;

namespace ShockwaveFlash.Tags.Text;

public sealed class DefineEditTextTag : Tag
{
    public ushort Id { get; set; }

    public Rectangle Bounds { get; set; }

    public ushort? FontId { get; set; }

    public string? FontClass { get; set; }

    public ushort? Height { get; set; }

    public Color? Color { get; set; }

    public ushort? MaxLength { get; set; }

    public TextLayout? Layout { get; set; }

    public string VariableName { get; set; }

    public string? InitialText { get; set; }

    public EditTextFlags Flags { get; set; }

    public bool HasVariableName =>
        !string.IsNullOrEmpty(VariableName);

    public bool HasHeight =>
        HasFont || HasFontClass;

    [MemberNotNullWhen(true, nameof(FontId))]
    public bool HasFont =>
        FontId is not null;

    [MemberNotNullWhen(true, nameof(MaxLength))]
    public bool HasMaxLength =>
        MaxLength is not null;

    [MemberNotNullWhen(true, nameof(Color))]
    public bool HasTextColor =>
        Color is not null;

    public bool ReadOnly =>
        Flags.HasFlag(EditTextFlags.ReadOnly);

    public bool Password =>
        Flags.HasFlag(EditTextFlags.Password);

    public bool Multiline =>
        Flags.HasFlag(EditTextFlags.Multiline);

    public bool WordWrap =>
        Flags.HasFlag(EditTextFlags.WordWrap);

    [MemberNotNullWhen(true, nameof(InitialText))]
    public bool HasText =>
        InitialText is not null;

    public bool UseOutlines =>
        Flags.HasFlag(EditTextFlags.UseOutlines);

    public bool Html =>
        Flags.HasFlag(EditTextFlags.Html);

    public bool WasStatic =>
        Flags.HasFlag(EditTextFlags.WasStatic);

    public bool Border =>
        Flags.HasFlag(EditTextFlags.Border);

    public bool NoSelect =>
        Flags.HasFlag(EditTextFlags.NoSelect);

    [MemberNotNullWhen(true, nameof(Layout))]
    public bool HasLayout =>
        Layout is not null;

    public bool AutoSize =>
        Flags.HasFlag(EditTextFlags.AutoSize);

    [MemberNotNullWhen(true, nameof(FontClass))]
    public bool HasFontClass =>
        FontClass is not null;

    public DefineEditTextTag(TagMetadata metadata, ushort id, Rectangle bounds, ushort? fontId, string? fontClass, ushort? height, Color? color, ushort? maxLength, TextLayout? layout, string variableName, string? initialText, EditTextFlags flags) : base(metadata)
    {
        Id = id;
        Bounds = bounds;
        FontId = fontId;
        FontClass = fontClass;
        Height = height;
        Color = color;
        MaxLength = maxLength;
        Layout = layout;
        VariableName = variableName;
        InitialText = initialText;
        Flags = flags;
    }

    public static DefineEditTextTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var bounds = Rectangle.Decode(reader);
        var flags = (EditTextFlags)reader.ReadUInt16();
        ushort? fontId = flags.HasFlag(EditTextFlags.HasFont) ? reader.ReadUInt16() : null;
        var fontClass = flags.HasFlag(EditTextFlags.HasFontClass) ? reader.ReadNullTerminatedString() : null;
        ushort? height = (flags & (EditTextFlags.HasFont | EditTextFlags.HasFontClass)) is not 0 ? reader.ReadUInt16() : null;
        Color? color = flags.HasFlag(EditTextFlags.HasTextColor) ? Types.Color.DecodeRgba(reader) : null;
        ushort? maxLength = flags.HasFlag(EditTextFlags.HasMaxLength) ? reader.ReadUInt16() : null;
        var layout = flags.HasFlag(EditTextFlags.HasLayout) ? TextLayout.Decode(reader) : null;
        var variableName = reader.ReadNullTerminatedString();
        var initialText = flags.HasFlag(EditTextFlags.HasText) ? reader.ReadNullTerminatedString() : null;

        return new DefineEditTextTag(metadata, id, bounds, fontId, fontClass, height, color, maxLength, layout, variableName, initialText, flags);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        Bounds.Encode(writer);
        writer.WriteUInt16((ushort)ComputeFlags());

        if (FontId is { } fontId)
            writer.WriteUInt16(fontId);

        if (FontClass is { } fontClass)
            writer.WriteNullTerminatedString(fontClass);

        if (HasHeight)
            writer.WriteUInt16(Height ?? (ushort)0);

        if (Color is { } color)
            color.EncodeRgba(writer);

        if (MaxLength is { } maxLength)
            writer.WriteUInt16(maxLength);

        if (Layout is { } layout)
            layout.Encode(writer);

        writer.WriteNullTerminatedString(VariableName);

        if (InitialText is { } initialText)
            writer.WriteNullTerminatedString(initialText);
    }

    private EditTextFlags ComputeFlags()
    {
        const EditTextFlags presence =
            EditTextFlags.HasFont | EditTextFlags.HasFontClass | EditTextFlags.HasMaxLength |
            EditTextFlags.HasTextColor | EditTextFlags.HasText | EditTextFlags.HasLayout;

        var flags = Flags & ~presence;

        if (HasFont)
            flags |= EditTextFlags.HasFont;

        if (HasFontClass)
            flags |= EditTextFlags.HasFontClass;

        if (HasMaxLength)
            flags |= EditTextFlags.HasMaxLength;

        if (HasTextColor)
            flags |= EditTextFlags.HasTextColor;

        if (HasText)
            flags |= EditTextFlags.HasText;

        if (HasLayout)
            flags |= EditTextFlags.HasLayout;

        return flags;
    }
}
