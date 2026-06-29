using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ShockwaveFlash.SourceGenerators;

internal readonly record struct LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public Location ToLocation()
    {
        return Location.Create(FilePath, TextSpan, LineSpan);
    }

    public static LocationInfo? CreateFrom(Location location)
    {
        return location.SourceTree is null
            ? null
            : new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }

}
