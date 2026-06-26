using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Tags.Shape;

public sealed class DefineShape4Tag : DefineShapeTag
{
    public Rectangle EdgeBounds { get; set; }

    public ShapeFlags Flags { get; set; }

    public DefineShape4Tag(TagMetadata metadata, ushort shapeId, Rectangle shapeBounds, Rectangle edgeBounds, ShapeFlags flags, ShapeStyles styles, IReadOnlyList<ShapeRecord> shapes)
        : base(metadata, shapeId, shapeBounds, styles, shapes)
    {
        EdgeBounds = edgeBounds;
        Flags = flags;
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        Encode(writer, swfVersion, 4);
    }
}
