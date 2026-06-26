using ShockwaveFlash;
using ShockwaveFlash.Avm1;
using ShockwaveFlash.Avm1.Types;
using ShockwaveFlash.Tags.Action;
using Shouldly;
using Avm1Action = ShockwaveFlash.Avm1.Action;

namespace ShockwaveFlash.Tests;

public sealed class Avm1RoundTripTests
{
    private static IReadOnlyList<string> Corpus()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "data")))
            dir = Path.GetDirectoryName(dir);

        if (dir is null)
            return [];

        return Directory.EnumerateFiles(Path.Combine(dir, "data"), "*.swf", SearchOption.AllDirectories)
            .Where(p => p.Replace('\\', '/') is var u && (u.Contains("/langs/", StringComparison.Ordinal) || u.Contains("/maps/", StringComparison.Ordinal)))
            .ToList();
    }

    [Fact]
    public void Encoder_is_byte_identical_to_the_decoder()
    {
        var failures = new List<string>();

        foreach (var path in Corpus())
        {
            var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));

            foreach (var tag in swf.Tags.OfType<DoActionTag>())
            {
                var actions = Avm1Action.DecodeCollection(tag.Data, swf.Header.Version);
                var bytes = Avm1Action.EncodeCollection(actions, swf.Header.Version);

                if (!bytes.Span.SequenceEqual(tag.Data.Span))
                    failures.Add(Path.GetFileName(path));
            }
        }

        failures.ShouldBeEmpty();
    }

    [Fact]
    public void Emitter_round_trips_the_evaluated_globals()
    {
        var failures = new List<string>();

        foreach (var path in Corpus())
        {
            var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));
            var version = swf.Header.Version;

            var globals = Avm1Machine.Run(swf.Tags.OfType<DoActionTag>().First().Data, version);
            var reGlobals = Avm1Machine.Run(Avm1Emitter.EmitBytes(globals, version), version);

            if (!DeepEquals(globals, reGlobals))
                failures.Add(Path.GetFileName(path));
        }

        failures.ShouldBeEmpty();
    }

    [Fact]
    public void Strict_evaluation_succeeds_on_data_scripts()
    {
        foreach (var path in Corpus())
        {
            var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));
            var tag = swf.Tags.OfType<DoActionTag>().First();

            Should.NotThrow(() => Avm1Machine.Run(tag.Data, swf.Header.Version, strict: true));
        }
    }

    [Fact]
    public void Edit_via_facade_writes_back_a_readable_swf()
    {
        var path = Corpus().FirstOrDefault(p => p.Contains("emotes", StringComparison.Ordinal));
        if (path is null)
            return;

        var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));
        var version = swf.Header.Version;
        var tag = swf.Tags.OfType<DoActionTag>().First();

        var globals = tag.Evaluate(version);
        globals["EM"].AsObject["1"].AsObject["n"] = "Position assise";

        var rebuilt = swf.ReplaceTag(tag, tag.WithGlobals(globals, version)).Assemble();

        var reparsed = ShockwaveFlashFile.Disassemble(rebuilt);
        var reGlobals = reparsed.Tags.OfType<DoActionTag>().First().Evaluate(version);

        reGlobals["EM"].AsObject["1"].AsObject["n"].AsString.ShouldBe("Position assise");
    }

    private static bool DeepEquals(Avm1Value a, Avm1Value b)
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
}
