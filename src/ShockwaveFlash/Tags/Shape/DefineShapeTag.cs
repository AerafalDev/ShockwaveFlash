using ShockwaveFlash.Exceptions;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Tags.Shape;

public class DefineShapeTag : Tag
{
    public ushort ShapeId { get; set; }

    public Rectangle ShapeBounds { get; set; }

    public ShapeStyles Styles { get; set; }

    public IReadOnlyList<ShapeRecord> Shapes { get; set; }

    public DefineShapeTag(TagMetadata metadata, ushort shapeId, Rectangle shapeBounds, ShapeStyles styles, IReadOnlyList<ShapeRecord> shapes) : base(metadata)
    {
        ShapeId = shapeId;
        ShapeBounds = shapeBounds;
        Styles = styles;
        Shapes = shapes;
    }

    public static DefineShapeTag Decode(MemoryReader reader, TagMetadata metadata, byte swfVersion, byte shapeVersion)
    {
        var shapeId = reader.ReadUInt16();
        var shapeBounds = Rectangle.Decode(reader);
        var edgeBounds = new Rectangle();
        var flags = ShapeFlags.HasNonScalingStrokes;

        if (shapeVersion >= 4)
        {
            edgeBounds = Rectangle.Decode(reader);
            flags = (ShapeFlags)reader.ReadUInt8();
        }

        var (styles, numFillBits, numLineBits) = ShapeStyles.Decode(reader, swfVersion, shapeVersion);

        var shapeContext = new ShapeContext(swfVersion, shapeVersion, numFillBits, numLineBits);

        var records = new List<ShapeRecord>();

        var bits = new BitReader();

        var record = ShapeRecord.Decode(reader, bits, shapeContext);

        while (record is not EndShapeRecord)
        {
            records.Add(record);
            record = ShapeRecord.Decode(reader, bits, shapeContext);
        }

        records.Add(record);

        return shapeVersion switch
        {
            1 => new DefineShapeTag(metadata, shapeId, shapeBounds, styles, records),
            2 => new DefineShape2Tag(metadata, shapeId, shapeBounds, styles, records),
            3 => new DefineShape3Tag(metadata, shapeId, shapeBounds, styles, records),
            4 => new DefineShape4Tag(metadata, shapeId, shapeBounds, edgeBounds, flags, styles, records),
            _ => throw new SwfFormatException($"Shape version {shapeVersion} is not supported.")
        };
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        Encode(writer, swfVersion, 1);
    }

    protected void Encode(MemoryWriter writer, byte swfVersion, byte shapeVersion)
    {
        writer.WriteUInt16(ShapeId);
        ShapeBounds.Encode(writer);

        if (shapeVersion >= 4 && this is DefineShape4Tag shape4)
        {
            shape4.EdgeBounds.Encode(writer);
            writer.WriteUInt8((byte)shape4.Flags);
        }

        var (numFillBits, numLineBits) = Styles.Encode(writer, swfVersion, shapeVersion);

        var context = new ShapeContext(swfVersion, shapeVersion, numFillBits, numLineBits);

        ShapeRecord.EncodeCollection(writer, Shapes, context);
    }
}
