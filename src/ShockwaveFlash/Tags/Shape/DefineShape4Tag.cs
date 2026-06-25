using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Tags.Shape;

public sealed record DefineShape4Tag(TagMetadata Metadata, ushort ShapeId, Rectangle ShapeBounds, Rectangle EdgeBounds, ShapeFlags Flags, ShapeStyles Styles, IReadOnlyList<ShapeRecord> Shapes) : DefineShapeTag(Metadata, ShapeId, ShapeBounds, Styles, Shapes)
{
    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        Encode(writer, swfVersion, 4);
    }
}
