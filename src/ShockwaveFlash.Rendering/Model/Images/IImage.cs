using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Images;

public interface IImage
{
    Rectangle Bounds { get; }

    ReadOnlyMemory<byte> ToPng();

    string ToBase64Data();

    IImage TransformColors(ColorTransform colorTransform);
}
