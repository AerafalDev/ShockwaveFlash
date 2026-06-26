namespace ShockwaveFlash.Benchmarks;

internal static class BenchmarkCorpus
{
    private static readonly Dictionary<string, string> Files = new()
    {
        ["small"] = "dofus_main_6.0_2.73.3.14/mounts/1.swf",
        ["medium"] = "retro_main_6.0_1.48.15.5506.412-03740e5/core.swf",
        ["large"] = "dofus_main_6.0_2.73.3.14/DofusInvoker.swf",
    };

    public static string Root()
    {
        var dir = AppContext.BaseDirectory;

        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "data");

            if (Directory.Exists(candidate))
                return candidate;

            dir = Path.GetDirectoryName(dir);
        }

        throw new DirectoryNotFoundException("Could not locate the data/ corpus directory.");
    }

    public static byte[] Load(string label)
    {
        return File.ReadAllBytes(Path.Combine(Root(), Files[label]));
    }

    public static IReadOnlyList<byte[]> LoadAll()
    {
        return Directory.EnumerateFiles(Root(), "*.swf", SearchOption.AllDirectories)
            .Select(static path => File.ReadAllBytes(path))
            .ToList();
    }
}
