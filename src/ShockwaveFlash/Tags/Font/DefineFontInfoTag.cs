// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Font;

namespace ShockwaveFlash.Tags.Font;

public sealed record DefineFontInfoTag(TagMetadata Metadata, ushort Id, string Name, FontInfoFlags Flags, IReadOnlyList<ushort> CodeTable) : Tag(Metadata)
{
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

    public static DefineFontInfoTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var name = reader.ReadLengthPrefixedString();
        var flags = (FontInfoFlags)reader.ReadUInt8();
        var codeTable = new List<ushort>();

        while (reader.Remaining > 0)
            codeTable.Add(flags.HasFlag(FontInfoFlags.HasWideCodes) ? reader.ReadUInt16() : reader.ReadUInt8());

        return new DefineFontInfoTag(metadata, id, name, flags, codeTable);
    }
}
