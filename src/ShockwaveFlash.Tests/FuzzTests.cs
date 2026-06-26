using ShockwaveFlash;
using ShockwaveFlash.Exceptions;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class FuzzTests
{
    [Fact]
    public void Truncating_any_input_only_throws_swf_exceptions()
    {
        var failures = new List<string>();

        foreach (var (name, data) in Inputs())
        {
            var step = Math.Max(1, data.Length / 200);

            for (var length = 0; length <= data.Length; length += step)
                Check(failures, data[..length], $"{name} truncated to {length}/{data.Length}");
        }

        failures.ShouldBeEmpty();
    }

    [Fact]
    public void Flipping_any_byte_only_throws_swf_exceptions()
    {
        var failures = new List<string>();

        foreach (var (name, data) in Inputs())
        {
            var step = Math.Max(1, data.Length / 150);

            for (var i = 0; i < data.Length; i += step)
            {
                var mutated = (byte[])data.Clone();
                mutated[i] ^= 0xFF;
                Check(failures, mutated, $"{name} byte {i} flipped");
            }
        }

        failures.ShouldBeEmpty();
    }

    private static void Check(List<string> failures, byte[] data, string description)
    {
        try
        {
            _ = ShockwaveFlashFile.Disassemble(data);
        }
        catch (SwfException)
        {
        }
        catch (Exception ex)
        {
            failures.Add($"{description}: expected a SwfException but got {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static IEnumerable<(string Name, byte[] Data)> Inputs()
    {
        yield return ("minimal", SwfTestData.MinimalUncompressed());
        yield return ("do-action", SwfTestData.WithDoAction());
        yield return ("compressed", SwfTestData.ToZlibCompressed(SwfTestData.MinimalUncompressed()));

        foreach (var path in Corpus.Files().OrderBy(static path => new FileInfo(path).Length).Take(8))
            yield return (Path.GetFileName(path), File.ReadAllBytes(path));
    }
}
