namespace ShockwaveFlash.Tests;

internal static class Corpus
{
    public static string? Root()
    {
        var dir = AppContext.BaseDirectory;

        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "data");

            if (Directory.Exists(candidate))
                return candidate;

            dir = Path.GetDirectoryName(dir);
        }

        return null;
    }

    public static IReadOnlyList<string> Files()
    {
        var root = Root();

        return root is null
            ? []
            : Directory.EnumerateFiles(root, "*.swf", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToList();
    }
}
