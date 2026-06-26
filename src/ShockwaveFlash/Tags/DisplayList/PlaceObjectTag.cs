using ShockwaveFlash.Types;

namespace ShockwaveFlash.Tags.DisplayList;

public sealed class PlaceObjectTag : Tag
{
    public ushort Id { get; set; }

    public ushort Depth { get; set; }

    public Matrix Matrix { get; set; }

    public ColorTransform? ColorTransform { get; set; }

    public PlaceObjectTag(TagMetadata metadata, ushort id, ushort depth, Matrix matrix, ColorTransform? colorTransform) : base(metadata)
    {
        Id = id;
        Depth = depth;
        Matrix = matrix;
        ColorTransform = colorTransform;
    }

    public static PlaceObjectTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var depth = reader.ReadUInt16();
        var matrix = Matrix.Decode(reader);

        ColorTransform? colorTransform = reader.Remaining > 0
            ? Types.ColorTransform.DecodeRgb(reader)
            : null;

        return new PlaceObjectTag(metadata, id, depth, matrix, colorTransform);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteUInt16(Depth);
        Matrix.Encode(writer);

        if (ColorTransform is { } colorTransform)
            colorTransform.EncodeRgb(writer);
    }
}
