using ShockwaveFlash;
using ShockwaveFlash.Avm1;
using ShockwaveFlash.Avm1.Types;
using ShockwaveFlash.Tests.Models;
using ShockwaveFlash.Tags.Action;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class LangsTypedRoundTripTests
{
    [Fact]
    public void All_langs_globals_round_trip_through_the_value_tree()
    {
        var files = Avm1Trees.LangsFiles();
        files.ShouldNotBeEmpty();

        var failures = new List<string>();

        foreach (var path in files)
        {
            var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));
            var version = swf.Header.Version;
            var tag = swf.Tags.OfType<DoActionTag>().FirstOrDefault();
            if (tag is null)
                continue;

            var globals = tag.Evaluate(version);
            var reGlobals = Avm1Machine.Run(Avm1Emitter.EmitBytes(globals, version), version);

            if (!Avm1Trees.DeepEquals(globals, reGlobals))
                failures.Add(Path.GetFileName(path));
        }

        failures.ShouldBeEmpty();
    }

    [Fact]
    public void Generated_Emote_mapper_round_trips_real_emote_data_across_langs()
    {
        var files = Avm1Trees.LangsFiles();
        var matched = 0;

        foreach (var path in files)
        {
            var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));
            var version = swf.Header.Version;
            var tag = swf.Tags.OfType<DoActionTag>().FirstOrDefault();
            if (tag is null)
                continue;

            var globals = tag.Evaluate(version);

            foreach (var (_, value) in globals.Members)
            {
                if (value is not Avm1Object container)
                    continue;

                foreach (var (id, leaf) in container.Members)
                {
                    if (!IsEmoteShaped(leaf, out var entry))
                        continue;

                    var roundTripped = Emote.FromAvm1Object(entry).ToAvm1Object();
                    Avm1Trees.DeepEquals(entry, roundTripped).ShouldBeTrue($"{Path.GetFileName(path)} / {id}");
                    matched++;
                }
            }
        }

        matched.ShouldBeGreaterThan(0);
    }

    private static bool IsEmoteShaped(Avm1Value value, out Avm1Object entry)
    {
        entry = null!;

        if (value is not Avm1Object table || table.Members.Count != 2)
            return false;
        if (!table.Members.TryGetValue("s", out var shortcut) || shortcut is not Avm1String)
            return false;
        if (!table.Members.TryGetValue("n", out var name) || name is not Avm1String)
            return false;

        entry = table;
        return true;
    }
}
