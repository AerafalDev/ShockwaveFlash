using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Tests.Serialization;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class GeneratedLangsRoundTripTests
{
    [Fact] public void Alignment() => AssertFile(DofusLangContext.Default.AlignmentFile, "alignment");

    [Fact] public void Audio() => AssertFile(DofusLangContext.Default.AudioFile, "audio");

    [Fact] public void Classes() => AssertFile(DofusLangContext.Default.ClassesFile, "classes");

    [Fact] public void Crafts() => AssertFile(DofusLangContext.Default.CraftsFile, "crafts");

    [Fact] public void Dialog() => AssertFile(DofusLangContext.Default.DialogFile, "dialog");

    [Fact] public void Dungeons() => AssertFile(DofusLangContext.Default.DungeonsFile, "dungeons");

    [Fact] public void Effects() => AssertFile(DofusLangContext.Default.EffectsFile, "effects");

    [Fact] public void Emotes() => AssertFile(DofusLangContext.Default.EmotesFile, "emotes");

    [Fact] public void FightChallenge() => AssertFile(DofusLangContext.Default.FightChallengeFile, "fightChallenge");

    [Fact] public void Guilds() => AssertFile(DofusLangContext.Default.GuildsFile, "guilds");

    [Fact] public void Hints() => AssertFile(DofusLangContext.Default.HintsFile, "hints");

    [Fact] public void Houses() => AssertFile(DofusLangContext.Default.HousesFile, "houses");

    [Fact] public void InteractiveObjects() => AssertFile(DofusLangContext.Default.InteractiveObjectsFile, "interactiveobjects");

    [Fact] public void Items() => AssertFile(DofusLangContext.Default.ItemsFile, "items");

    [Fact] public void Itemsets() => AssertFile(DofusLangContext.Default.ItemsetsFile, "itemsets");

    [Fact] public void Itemstats() => AssertFile(DofusLangContext.Default.ItemstatsFile, "itemstats");

    [Fact] public void Jobs() => AssertFile(DofusLangContext.Default.JobsFile, "jobs");

    [Fact] public void Kb() => AssertFile(DofusLangContext.Default.KbFile, "kb");

    [Fact] public void Maps() => AssertFile(DofusLangContext.Default.MapsFile, "maps");

    [Fact] public void Monsters() => AssertFile(DofusLangContext.Default.MonstersFile, "monsters");

    [Fact] public void Names() => AssertFile(DofusLangContext.Default.NamesFile, "names");

    [Fact] public void Npc() => AssertFile(DofusLangContext.Default.NpcFile, "npc");

    [Fact] public void Pvp() => AssertFile(DofusLangContext.Default.PvpFile, "pvp");

    [Fact] public void Quests() => AssertFile(DofusLangContext.Default.QuestsFile, "quests");

    [Fact] public void Ranks() => AssertFile(DofusLangContext.Default.RanksFile, "ranks");

    [Fact] public void Rides() => AssertFile(DofusLangContext.Default.RidesFile, "rides");

    [Fact] public void Scripts() => AssertFile(DofusLangContext.Default.ScriptsFile, "scripts");

    [Fact] public void Servers() => AssertFile(DofusLangContext.Default.ServersFile, "servers");

    [Fact] public void Shortcuts() => AssertFile(DofusLangContext.Default.ShortcutsFile, "shortcuts");

    [Fact] public void Skills() => AssertFile(DofusLangContext.Default.SkillsFile, "skills");

    [Fact] public void SpeakingItems() => AssertFile(DofusLangContext.Default.SpeakingItemsFile, "speakingitems");

    [Fact] public void Spells() => AssertFile(DofusLangContext.Default.SpellsFile, "spells");

    [Fact] public void States() => AssertFile(DofusLangContext.Default.StatesFile, "states");

    [Fact] public void Subtitles() => AssertFile(DofusLangContext.Default.SubtitlesFile, "subtitles");

    [Fact] public void Timezones() => AssertFile(DofusLangContext.Default.TimezonesFile, "timezones");

    [Fact] public void Titles() => AssertFile(DofusLangContext.Default.TitlesFile, "titles");

    [Fact] public void Ttg() => AssertFile(DofusLangContext.Default.TtgFile, "ttg");

    private static void AssertFile<T>(Avm1TypeInfo<T> typeInfo, string category)
    {
        var files = Avm1Trees.LangsFiles(category).ToList();
        files.ShouldNotBeEmpty($"no langs files for category '{category}'");

        foreach (var path in files)
        {
            var globals = Avm1Trees.Globals(path);
            var expected = Avm1Trees.DataOnly(globals);
            var actual = Avm1Serializer.Serialize(Avm1Serializer.Deserialize(globals, typeInfo), typeInfo);
            Avm1Trees.DeepEquals(expected, actual).ShouldBeTrue($"{category}: {Path.GetFileName(path)}");
        }
    }
}
