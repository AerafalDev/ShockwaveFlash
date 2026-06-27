namespace ShockwaveFlash.Rendering.Processing;

public interface IFontResolver
{
    ResolvedFont? ResolveFont(int fontId);
}
