using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Filter;

namespace ShockwaveFlash.Rendering.Scene;

public sealed record FrameObject
{
    public required int Depth { get; init; }

    public required IDrawable Drawable { get; init; }

    public required Matrix Matrix { get; init; }

    public ColorTransform? ColorTransform { get; init; }

    public int? ClipDepth { get; init; }

    public BlendMode BlendMode { get; init; } = BlendMode.Normal;

    public bool Visible { get; init; } = true;

    public int? Ratio { get; init; }

    public string? Name { get; init; }

    public IReadOnlyList<Filter> Filters { get; init; } = [];
}
