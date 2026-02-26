// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Text;

namespace ShockwaveFlash.Tags;

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

    public static DefineEditTextTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var characterId = reader.ReadUInt16();
        var bounds = Rectangle.Decode(ref reader);
        var flags = (EditTextFlags)reader.ReadUInt16();
        ushort? fontId = flags.HasFlag(EditTextFlags.HasFont) ? reader.ReadUInt16() : null;
        var fontClass = flags.HasFlag(EditTextFlags.HasFontClass) ? reader.ReadNullTerminatedString() : null;
        ushort? height = (flags & (EditTextFlags.HasFont | EditTextFlags.HasFontClass)) is not 0 ? reader.ReadUInt16() : null;
        Color? color = flags.HasFlag(EditTextFlags.HasTextColor) ? Types.Color.DecodeRgba(ref reader) : null;
        ushort? maxLength = flags.HasFlag(EditTextFlags.HasMaxLength) ? reader.ReadUInt16() : null;
        var layout = flags.HasFlag(EditTextFlags.HasLayout) ? TextLayout.Decode(ref reader) : null;
        var variableName = reader.ReadNullTerminatedString();
        var initialText = flags.HasFlag(EditTextFlags.HasText) ? reader.ReadNullTerminatedString() : null;

        return new DefineEditTextTag(metadata, characterId, bounds, fontId, fontClass, height, color, maxLength, layout, variableName, initialText, flags);
    }
}
