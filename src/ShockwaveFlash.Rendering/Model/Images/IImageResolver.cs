namespace ShockwaveFlash.Rendering.Model.Images;

public interface IImageResolver
{
    IImage? ResolveImage(int characterId);
}
