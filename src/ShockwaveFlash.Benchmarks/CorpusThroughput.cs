using System.Diagnostics;

namespace ShockwaveFlash.Benchmarks;

internal static class CorpusThroughput
{
    public static void Run(string directory, int rounds)
    {
        var files = Directory.EnumerateFiles(directory, "*.swf", SearchOption.AllDirectories)
            .Select(File.ReadAllBytes)
            .ToList();

        var bytes = files.Sum(static file => (long)file.Length);

        var tags = 0;
        var parse = Best(rounds, () =>
        {
            var total = 0;

            foreach (var data in files)
            {
                try
                {
                    total += ShockwaveFlashFile.Disassemble(data).Tags.Count;
                }
                catch
                {
                }
            }

            tags = total;
        });

        var roundTrip = Best(rounds, () =>
        {
            foreach (var data in files)
            {
                try
                {
                    _ = ShockwaveFlashFile.Disassemble(data).Assemble();
                }
                catch
                {
                }
            }
        });

        Console.WriteLine($"impl=ours files={files.Count} bytes={bytes} parse_ms={parse:F2} roundtrip_ms={roundTrip:F2} tags={tags}");
    }

    private static double Best(int rounds, System.Action action)
    {
        var best = double.MaxValue;

        for (var round = 0; round < rounds; round++)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();

            if (stopwatch.Elapsed.TotalMilliseconds < best)
                best = stopwatch.Elapsed.TotalMilliseconds;
        }

        return best;
    }
}
