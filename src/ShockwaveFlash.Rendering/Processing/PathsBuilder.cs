using ShockwaveFlash.Rendering.Model.Shapes;

namespace ShockwaveFlash.Rendering.Processing;

internal sealed record StyleSlot(PathStyle Style, bool Reverse, string Key);

internal sealed class OpenPath
{
    public PathStyle Style { get; }

    public List<IEdge> Edges { get; }

    public OpenPath(PathStyle style, List<IEdge> edges)
    {
        Style = style;
        Edges = edges;
    }
}

internal sealed class PathsBuilder
{
    private readonly Dictionary<string, OpenPath> _open = new(StringComparer.Ordinal);

    private readonly List<OpenPath> _closed = [];

    public void Merge(StyleSlot? fill0, StyleSlot? fill1, StyleSlot? line, IReadOnlyList<IEdge> edges)
    {
        if (edges.Count is 0)
            return;

        Push(fill0, edges);
        Push(fill1, edges);
        Push(line, edges);
    }

    public void Close()
    {
        _closed.AddRange(_open.Values);
        _open.Clear();
    }

    public IReadOnlyList<ShapePath> Export()
    {
        Close();

        var fills = new List<ShapePath>();
        var lines = new List<ShapePath>();

        foreach (var open in _closed)
        {
            var path = new ShapePath(Stitch(open.Edges), open.Style);
            (open.Style.IsLine ? lines : fills).Add(path);
        }

        return [.. fills, .. lines];
    }

    private void Push(StyleSlot? slot, IReadOnlyList<IEdge> edges)
    {
        if (slot is null)
            return;

        var run = slot.Reverse ? ReverseRun(edges) : [.. edges];

        if (_open.TryGetValue(slot.Key, out var open))
            open.Edges.AddRange(run);
        else
            _open[slot.Key] = new OpenPath(slot.Style, run);
    }

    private static List<IEdge> ReverseRun(IReadOnlyList<IEdge> edges)
    {
        var result = new List<IEdge>(edges.Count);

        for (var i = edges.Count - 1; i >= 0; i--)
            result.Add(edges[i].Reverse());

        return result;
    }

    private static List<IEdge> Stitch(List<IEdge> edges)
    {
        if (edges.Count <= 1)
            return edges;

        var remaining = new LinkedList<IEdge>(edges);
        var result = new List<IEdge>(edges.Count);

        var current = remaining.First!.Value;
        remaining.RemoveFirst();
        result.Add(current);

        while (remaining.Count > 0)
        {
            var node = remaining.First;
            var found = false;

            while (node is not null)
            {
                var edge = node.Value;

                if (edge.FromX == current.ToX && edge.FromY == current.ToY)
                {
                    result.Add(edge);
                    current = edge;
                    remaining.Remove(node);
                    found = true;
                    break;
                }

                if (edge.ToX == current.ToX && edge.ToY == current.ToY)
                {
                    var reversed = edge.Reverse();
                    result.Add(reversed);
                    current = reversed;
                    remaining.Remove(node);
                    found = true;
                    break;
                }

                node = node.Next;
            }

            if (found)
                continue;

            current = remaining.First!.Value;
            remaining.RemoveFirst();
            result.Add(current);
        }

        return result;
    }
}
