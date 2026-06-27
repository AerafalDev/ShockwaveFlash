using ShockwaveFlash.Types.Font;

namespace ShockwaveFlash.Rendering.Processing;

public sealed record ResolvedFont(IReadOnlyList<FontGlyph> Glyphs, float EmSquare, int Ascent, int Descent, int Leading)
{
    private Dictionary<ushort, int>? _byCode;

    public int IndexForCode(ushort code)
    {
        _byCode ??= BuildCodeMap();
        return _byCode.TryGetValue(code, out var index) ? index : -1;
    }

    private Dictionary<ushort, int> BuildCodeMap()
    {
        var map = new Dictionary<ushort, int>(Glyphs.Count);

        for (var i = 0; i < Glyphs.Count; i++)
            map[Glyphs[i].Code] = i;

        return map;
    }
}
