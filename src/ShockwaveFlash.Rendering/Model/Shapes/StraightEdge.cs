namespace ShockwaveFlash.Rendering.Model.Shapes;

public sealed record StraightEdge(int FromX, int FromY, int ToX, int ToY) : IEdge
{
    public IEdge Reverse()
    {
        return new StraightEdge(ToX, ToY, FromX, FromY);
    }
}
