namespace ShockwaveFlash.Rendering.Model.Shapes;

public sealed record CurvedEdge(int FromX, int FromY, int ControlX, int ControlY, int ToX, int ToY) : IEdge
{
    public IEdge Reverse()
    {
        return new CurvedEdge(ToX, ToY, ControlX, ControlY, FromX, FromY);
    }
}
