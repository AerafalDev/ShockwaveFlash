using System.Diagnostics.CodeAnalysis;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Text;

namespace ShockwaveFlash.Tags.Text;

public sealed record DefineEditTextTag(
    TagMetadata Metadata,
    ushort Id,
    Rectangle Bounds,
    ushort? FontId,
    string? FontClass,
    ushort? Height,
    Color? Color,
    ushort? MaxLength,
    TextLayout? Layout,
    string VariableName,
    string? InitialText,
    EditTextFlags Flags) : Tag(Metadata)
{
    public bool HasVariableName =>
        !string.IsNullOrEmpty(VariableName);

    [MemberNotNullWhen(true, nameof(Height))]
    public bool HasHeight =>
        (Flags & (EditTextFlags.HasFont | EditTextFlags.HasFontClass)) is not 0;

    [MemberNotNullWhen(true, nameof(FontId))]
    public bool HasFont =>
        Flags.HasFlag(EditTextFlags.HasFont);

    [MemberNotNullWhen(true, nameof(MaxLength))]
    public bool HasMaxLength =>
        Flags.HasFlag(EditTextFlags.HasMaxLength);

    [MemberNotNullWhen(true, nameof(Color))]
    public bool HasTextColor =>
        Flags.HasFlag(EditTextFlags.HasTextColor);

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
        Flags.HasFlag(EditTextFlags.HasText);

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
        Flags.HasFlag(EditTextFlags.HasLayout);

    public bool AutoSize =>
        Flags.HasFlag(EditTextFlags.AutoSize);

    [MemberNotNullWhen(true, nameof(FontClass))]
    public bool HasFontClass =>
        Flags.HasFlag(EditTextFlags.HasFontClass);

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
        writer.WriteUInt16((ushort)Flags);

        if (HasFont && FontId is { } fontId)
            writer.WriteUInt16(fontId);

        if (HasFontClass && FontClass is { } fontClass)
            writer.WriteNullTerminatedString(fontClass);

        if (HasHeight && Height is { } height)
            writer.WriteUInt16(height);

        if (HasTextColor && Color is { } color)
            color.EncodeRgba(writer);

        if (HasMaxLength && MaxLength is { } maxLength)
            writer.WriteUInt16(maxLength);

        if (HasLayout && Layout is { } layout)
            layout.Encode(writer);

        writer.WriteNullTerminatedString(VariableName);

        if (HasText && InitialText is { } initialText)
            writer.WriteNullTerminatedString(initialText);
    }
}
