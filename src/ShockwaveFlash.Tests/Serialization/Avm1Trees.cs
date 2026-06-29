using ShockwaveFlash.Avm1;
using ShockwaveFlash.Avm1.Types;
using ShockwaveFlash.Tags.Action;

namespace ShockwaveFlash.Tests;

internal static class Avm1Trees
{
    public static IEnumerable<string> LangsFiles(string category)
    {
        return LangsFiles().Where(p => Path.GetFileName(p).StartsWith(category + "_", StringComparison.Ordinal));
    }

    public static Avm1Object Globals(string path)
    {
        var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));
        var version = swf.Header.Version;
        var tag = swf.Tags.OfType<DoActionTag>().First();
        return tag.Evaluate(version);
    }

    public static Avm1Object DataOnly(Avm1Object globals)
    {
        var result = new Avm1Object();

        foreach (var (key, value) in globals.Members)
        {
            if (key is not ("FILE_BEGIN" or "VERSION" or "FILE_END"))
                result.Members[key] = value;
        }

        return result;
    }


    public static bool DeepEquals(Avm1Value a, Avm1Value b)
    {
        return (a, b) switch
        {
            (Avm1String x, Avm1String y) => x.Value == y.Value,
            (Avm1Number x, Avm1Number y) => x.Value.Equals(y.Value),
            (Avm1Boolean x, Avm1Boolean y) => x.Value == y.Value,
            (Avm1Null, Avm1Null) => true,
            (Avm1Undefined, Avm1Undefined) => true,
            (Avm1Object x, Avm1Object y) => x.Members.Count == y.Members.Count && x.Members.All(kv => y.Members.TryGetValue(kv.Key, out var v) && DeepEquals(kv.Value, v)),
            (Avm1Array x, Avm1Array y) => x.Items.Count == y.Items.Count && x.Items.Zip(y.Items, DeepEquals).All(z => z),
            _ => false
        };
    }

    public static IReadOnlyList<string> LangsFiles()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "data")))
            dir = Path.GetDirectoryName(dir);

        if (dir is null)
            return [];

        return Directory.EnumerateFiles(Path.Combine(dir, "data"), "*.swf", SearchOption.AllDirectories)
            .Where(p => p.Replace('\\', '/').Contains("/langs/", StringComparison.Ordinal))
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();
    }
}
