using BenchmarkDotNet.Attributes;

namespace ShockwaveFlash.Benchmarks;

[MemoryDiagnoser]
public class ParseBenchmarks
{
    private byte[] _bytes = [];

    [Params("small", "medium", "large")]
    public string Size { get; set; } = "";

    [GlobalSetup]
    public void Setup()
    {
        _bytes = BenchmarkCorpus.Load(Size);
    }

    [Benchmark]
    public int Disassemble()
    {
        return ShockwaveFlashFile.Disassemble(_bytes).Tags.Count;
    }
}
