// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Tags;

public sealed record DefineFont4Tag(TagMetadata Metadata, ushort Id, bool IsItalic, bool IsBold, string Name, SpanSlice Data) : Tag(Metadata)
{
    public static DefineFont4Tag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var flags = reader.ReadUInt8();
        var hasFontData = (flags & 4) is not 0;
        var isItalic = (flags & 2) is not 0;
        var isBold = (flags & 1) is not 0;
        var name = reader.ReadNullTerminatedString();
        var data = hasFontData ? reader.SliceToEnd() : SpanSlice.Empty;

        return new DefineFont4Tag(metadata, id, isItalic, isBold, name, data);
    }
}
