using BenchmarkDotNet.Running;
using ShockwaveFlash.Benchmarks;

if (args.Length > 0 && args[0] is "throughput")
{
    var directory = args.Length > 1 ? args[1] : BenchmarkCorpus.Root();
    var rounds = args.Length > 2 ? int.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture) : 5;

    CorpusThroughput.Run(directory, rounds);
    return;
}

BenchmarkSwitcher.FromAssembly(typeof(ParseBenchmarks).Assembly).Run(args);
