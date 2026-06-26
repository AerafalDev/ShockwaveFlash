namespace ShockwaveFlash.Rendering.Model.Shapes;

public interface IEdge
{
    int FromX { get; }

    int FromY { get; }

    int ToX { get; }

    int ToY { get; }

    IEdge Reverse();
}
