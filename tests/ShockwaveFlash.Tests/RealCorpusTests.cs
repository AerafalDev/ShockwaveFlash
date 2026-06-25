// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class RealCorpusTests
{
    [Fact]
    public void Every_local_sample_disassembles_without_throwing()
    {
        var dataDir = FindDataDir();

        if (dataDir is null)
            return;

        var failures = new List<string>();

        foreach (var file in Directory.EnumerateFiles(dataDir, "*.swf", SearchOption.AllDirectories))
        {
            try
            {
                var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(file));
                _ = swf.Tags.Count;
            }
            catch (Exception e)
            {
                failures.Add($"{Path.GetFileName(file)}: {e.GetType().Name}: {e.Message}");
            }
        }

        failures.ShouldBeEmpty();
    }

    [Fact]
    public void Every_local_sample_round_trips_byte_for_byte()
    {
        var dataDir = FindDataDir();

        if (dataDir is null)
            return;

        var failures = new List<string>();

        foreach (var file in Directory.EnumerateFiles(dataDir, "*.swf", SearchOption.AllDirectories))
        {
            try
            {
                var first = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(file)).Assemble();
                var second = ShockwaveFlashFile.Disassemble(first).Assemble();

                if (!first.Span.SequenceEqual(second.Span))
                    failures.Add($"{Path.GetFileName(file)}: not idempotent");
            }
            catch (Exception e)
            {
                failures.Add($"{Path.GetFileName(file)}: {e.GetType().Name}: {e.Message}");
            }
        }

        failures.ShouldBeEmpty();
    }

    private static string? FindDataDir()
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
}
