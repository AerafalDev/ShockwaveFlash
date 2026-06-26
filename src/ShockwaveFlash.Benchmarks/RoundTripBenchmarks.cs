using BenchmarkDotNet.Attributes;

namespace ShockwaveFlash.Benchmarks;

[MemoryDiagnoser]
public class RoundTripBenchmarks
{
    private byte[] _bytes = [];
    private ShockwaveFlashFile _parsed = null!;

    [Params("small", "medium", "large")]
    public string Size { get; set; } = "";

    [GlobalSetup]
    public void Setup()
    {
        _bytes = BenchmarkCorpus.Load(Size);
        _parsed = ShockwaveFlashFile.Disassemble(_bytes);
    }

    [Benchmark(Baseline = true)]
    public int Parse()
    {
        return ShockwaveFlashFile.Disassemble(_bytes).Tags.Count;
    }

    [Benchmark]
    public int Assemble()
    {
        return _parsed.Assemble().Length;
    }

    [Benchmark]
    public int ParseThenAssemble()
    {
        return ShockwaveFlashFile.Disassemble(_bytes).Assemble().Length;
    }
}
