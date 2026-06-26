using ShockwaveFlash.Rendering.Model.Images;
using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Rendering.Scene;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Filter;
using Path = ShockwaveFlash.Rendering.Model.Shapes.Path;

namespace ShockwaveFlash.Rendering.Drawing;

public interface IDrawer
{
    void Area(Rectangle bounds);

    void Shape(Shape shape);

    void Image(IImage image);

    void Include(IDrawable drawable, Matrix matrix, int frame, IReadOnlyList<Filter> filters, BlendMode blendMode, string? name);

    string StartClip(IDrawable drawable, Matrix matrix, int frame);

    void EndClip(string clipId);

    void Path(Path path);
}

public interface IDrawer<out TResult> : IDrawer
{
    TResult Render();
}
