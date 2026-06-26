using ShockwaveFlash.Types.Font;

namespace ShockwaveFlash.Rendering.Processing;

public interface IFontResolver
{
    ResolvedFont? ResolveFont(int fontId);
}

public sealed record ResolvedFont(IReadOnlyList<FontGlyph> Glyphs, float EmSquare);
