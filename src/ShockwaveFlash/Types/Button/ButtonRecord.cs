using ShockwaveFlash.Types.DisplayList;

namespace ShockwaveFlash.Types.Button;

public sealed class ButtonRecord
{
    public ButtonStates States { get; set; }

    public ushort Id { get; set; }

    public ushort Depth { get; set; }

    public Matrix Matrix { get; set; }

    public ColorTransform ColorTransform { get; set; }

    public Filter.Filter[] Filters { get; set; }

    public BlendMode BlendMode { get; set; }

    public ButtonRecord(ButtonStates states, ushort id, ushort depth, Matrix matrix, ColorTransform colorTransform, Filter.Filter[] filters, BlendMode blendMode)
    {
        States = states;
        Id = id;
        Depth = depth;
        Matrix = matrix;
        ColorTransform = colorTransform;
        Filters = filters;
        BlendMode = blendMode;
    }

    public static ButtonRecord? Decode(MemoryReader reader, byte buttonVersion)
    {
        var flags = reader.ReadUInt8();

        if (flags is 0)
            return null;

        var states = (ButtonStates)flags;
        var id = reader.ReadUInt16();
        var depth = reader.ReadUInt16();
        var matrix = Matrix.Decode(reader);

        var colorTransform = buttonVersion >= 2
            ? ColorTransform.DecodeRgba(reader)
            : ColorTransform.Identity;

        var filters = Array.Empty<Filter.Filter>();

        if ((flags & 16) is not 0)
        {
            var numFilters = reader.ReadUInt8();

            filters = new Filter.Filter[numFilters];

            for (var i = 0; i < numFilters; i++)
                filters[i] = Filter.Filter.Decode(reader);
        }

        var blendMode = (flags & 32) is not 0
            ? BlendMode.Parse(reader.ReadUInt8())
            : BlendMode.Normal;

        return new ButtonRecord(states, id, depth, matrix, colorTransform, filters, blendMode);
    }

    public void Encode(MemoryWriter writer, byte buttonVersion)
    {
        var hasFilters = Filters.Length > 0;
        var hasBlendMode = BlendMode != BlendMode.Normal;

        var flags = (byte)States;

        if (hasFilters)
            flags |= 16;

        if (hasBlendMode)
            flags |= 32;

        writer.WriteUInt8(flags);
        writer.WriteUInt16(Id);
        writer.WriteUInt16(Depth);
        Matrix.Encode(writer);

        if (buttonVersion >= 2)
            ColorTransform.EncodeRgba(writer);

        if (hasFilters)
        {
            writer.WriteUInt8((byte)Filters.Length);

            foreach (var filter in Filters)
                filter.Encode(writer);
        }

        if (hasBlendMode)
            writer.WriteUInt8((byte)BlendMode);
    }
}
