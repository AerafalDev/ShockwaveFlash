// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.DisplayList;

namespace ShockwaveFlash.Types.Button;

public sealed record ButtonRecord(ButtonStates States, ushort Id, ushort Depth, Matrix Matrix, ColorTransform ColorTransform, Filter.Filter[] Filters, BlendMode BlendMode)
{
    public static ButtonRecord? Decode(ref SpanReader reader, byte buttonVersion)
    {
        var flags = reader.ReadUInt8();

        if (flags is 0)
            return null;

        var states = (ButtonStates)flags;
        var id = reader.ReadUInt16();
        var depth = reader.ReadUInt16();
        var matrix = Matrix.Decode(ref reader);

        var colorTransform = buttonVersion >= 2
            ? ColorTransform.DecodeRgba(ref reader)
            : ColorTransform.Identity;

        var filters = Array.Empty<Filter.Filter>();

        if ((flags & 16) is not 0)
        {
            var numFilters = reader.ReadUInt8();

            filters = new Filter.Filter[numFilters];

            for (var i = 0; i < numFilters; i++)
                filters[i] = Filter.Filter.Decode(ref reader);
        }

        var blendMode = (flags & 32) is not 0
            ? BlendMode.Parse(reader.ReadUInt8())
            : BlendMode.Normal;

        return new ButtonRecord(states, id, depth, matrix, colorTransform, filters, blendMode);
    }
}
