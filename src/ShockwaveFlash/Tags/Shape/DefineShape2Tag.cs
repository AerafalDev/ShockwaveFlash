using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Tags.Shape;

public sealed class DefineShape2Tag : DefineShapeTag
{
    public DefineShape2Tag(TagMetadata metadata, ushort shapeId, Rectangle shapeBounds, ShapeStyles styles, IReadOnlyList<ShapeRecord> shapes)
        : base(metadata, shapeId, shapeBounds, styles, shapes)
    {
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        Encode(writer, swfVersion, 2);
    }
}
