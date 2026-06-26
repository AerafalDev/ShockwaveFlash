using ShockwaveFlash;
using ShockwaveFlash.Tags;
using ShockwaveFlash.Tags.Shape;
using ShockwaveFlash.Types.Shape;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class CorpusRoundTripTests
{
    [Fact]
    public void Reparsing_the_assembled_movie_yields_a_deeply_equal_model()
    {
        var failures = new List<string>();

        foreach (var path in Corpus.Files())
        {
            var original = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));
            var reparsed = ShockwaveFlashFile.Disassemble(original.Assemble());

            if (original.Tags.Count != reparsed.Tags.Count)
            {
                failures.Add($"{Path.GetFileName(path)}: tag count {original.Tags.Count} vs {reparsed.Tags.Count}");
                continue;
            }

            for (var i = 0; i < original.Tags.Count; i++)
                if (!DeepEquality.Equals(original.Tags[i], reparsed.Tags[i], $"{Path.GetFileName(path)} Tags[{i}]:{original.Tags[i].GetType().Name}", out var where))
                {
                    failures.Add(where);
                    break;
                }
        }

        failures.ShouldBeEmpty();
    }

    [Fact]
    public void Every_define_shape_terminates_in_exactly_one_end_record()
    {
        var failures = new List<string>();

        foreach (var path in Corpus.Files())
        {
            var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path));

            foreach (var shape in swf.Tags.OfType<DefineShapeTag>())
            {
                var records = shape.Shapes;
                var endCount = records.Count(static record => record is EndShapeRecord);

                if (endCount != 1 || records.Count == 0 || records[^1] is not EndShapeRecord)
                    failures.Add($"{Path.GetFileName(path)} shape {shape.ShapeId}: {endCount} end records, last is {records.LastOrDefault()?.GetType().Name}");
            }
        }

        failures.ShouldBeEmpty();
    }

    [Fact]
    public void The_corpus_exercises_a_broad_census_of_tag_types()
    {
        if (Corpus.Files().Count == 0)
            return;

        var seen = new HashSet<string>();

        foreach (var path in Corpus.Files())
            foreach (var tag in ShockwaveFlashFile.Disassemble(File.ReadAllBytes(path)).Tags)
                seen.Add(tag.GetType().Name);

        // A guard so coverage cannot silently shrink: these tag types are known to occur in the corpus.
        string[] expected =
        [
            "DefineSpriteTag",
            "DefineShapeTag",
            "DefineShape2Tag",
            "DefineShape3Tag",
            "DefineShape4Tag",
            "DefineMorphShapeTag",
            "DefineMorphShape2Tag",
            "PlaceObject2Tag",
            "ShowFrameTag",
            "EndTag",
            "SetBackgroundColorTag",
            "DefineFont3Tag",
            "DefineTextTag",
            "DefineEditTextTag",
            "DefineButton2Tag",
            "DefineBitsLossless2Tag",
            "DefineBitsJpeg2Tag",
            "DefineBitsJpeg3Tag",
            "DefineBitsTag",
            "JpegTablesTag",
            "DoActionTag",
            "DoInitActionTag",
            "DoAbc2Tag",
            "SymbolClassTag",
            "ExportAssetsTag",
        ];

        var missing = expected.Where(name => !seen.Contains(name)).ToList();
        missing.ShouldBeEmpty();
        seen.Count.ShouldBeGreaterThanOrEqualTo(30);
    }
}
