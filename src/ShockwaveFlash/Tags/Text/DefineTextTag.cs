// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Text;

namespace ShockwaveFlash.Tags.Text;

public sealed record DefineTextTag(TagMetadata Metadata, ushort Id, Rectangle Bounds, Matrix Matrix, IReadOnlyList<TextRecord> Records) : Tag(Metadata)
{
    public static DefineTextTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var bounds = Rectangle.Decode(ref reader);
        var matrix = Matrix.Decode(ref reader);
        var numGlyphBits = reader.ReadUInt8();
        var numAdvanceBits = reader.ReadUInt8();

        var records = new List<TextRecord>();

        while (true)
        {
            var record = TextRecord.Decode(ref reader, 1, numGlyphBits, numAdvanceBits);

            if (record is null)
                break;

            records.Add(record);
        }

        return new DefineTextTag(metadata, id, bounds, matrix, records);
    }
}
