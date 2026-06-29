using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Tests.Models.Langs;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class ReflectionLangsRoundTripTests
{
    [Fact] public void Alignment() => AssertFile<AlignmentFile>("alignment");

    [Fact] public void Audio() => AssertFile<AudioFile>("audio");

    [Fact] public void Classes() => AssertFile<ClassesFile>("classes");

    [Fact] public void Crafts() => AssertFile<CraftsFile>("crafts");

    [Fact] public void Dialog() => AssertFile<DialogFile>("dialog");

    [Fact] public void Dungeons() => AssertFile<DungeonsFile>("dungeons");

    [Fact] public void Effects() => AssertFile<EffectsFile>("effects");

    [Fact] public void Emotes() => AssertFile<EmotesFile>("emotes");

    [Fact] public void FightChallenge() => AssertFile<FightChallengeFile>("fightChallenge");

    [Fact] public void Guilds() => AssertFile<GuildsFile>("guilds");

    [Fact] public void Hints() => AssertFile<HintsFile>("hints");

    [Fact] public void Houses() => AssertFile<HousesFile>("houses");

    [Fact] public void InteractiveObjects() => AssertFile<InteractiveObjectsFile>("interactiveobjects");

    [Fact] public void Items() => AssertFile<ItemsFile>("items");

    [Fact] public void Itemsets() => AssertFile<ItemsetsFile>("itemsets");

    [Fact] public void Itemstats() => AssertFile<ItemstatsFile>("itemstats");

    [Fact] public void Jobs() => AssertFile<JobsFile>("jobs");

    [Fact] public void Kb() => AssertFile<KbFile>("kb");

    [Fact] public void Maps() => AssertFile<MapsFile>("maps");

    [Fact] public void Monsters() => AssertFile<MonstersFile>("monsters");

    [Fact] public void Names() => AssertFile<NamesFile>("names");

    [Fact] public void Npc() => AssertFile<NpcFile>("npc");

    [Fact] public void Pvp() => AssertFile<PvpFile>("pvp");

    [Fact] public void Quests() => AssertFile<QuestsFile>("quests");

    [Fact] public void Ranks() => AssertFile<RanksFile>("ranks");

    [Fact] public void Rides() => AssertFile<RidesFile>("rides");

    [Fact] public void Scripts() => AssertFile<ScriptsFile>("scripts");

    [Fact] public void Servers() => AssertFile<ServersFile>("servers");

    [Fact] public void Shortcuts() => AssertFile<ShortcutsFile>("shortcuts");

    [Fact] public void Skills() => AssertFile<SkillsFile>("skills");

    [Fact] public void SpeakingItems() => AssertFile<SpeakingItemsFile>("speakingitems");

    [Fact] public void Spells() => AssertFile<SpellsFile>("spells");

    [Fact] public void States() => AssertFile<StatesFile>("states");

    [Fact] public void Subtitles() => AssertFile<SubtitlesFile>("subtitles");

    [Fact] public void Timezones() => AssertFile<TimezonesFile>("timezones");

    [Fact] public void Titles() => AssertFile<TitlesFile>("titles");

    [Fact] public void Ttg() => AssertFile<TtgFile>("ttg");

    private static void AssertFile<T>(string category)
    {
        var files = Avm1Trees.LangsFiles(category).ToList();
        files.ShouldNotBeEmpty($"no langs files for category '{category}'");

        foreach (var path in files)
        {
            var globals = Avm1Trees.Globals(path);
            var expected = Avm1Trees.DataOnly(globals);
            var actual = Avm1Serializer.Serialize(Avm1Serializer.Deserialize<T>(globals));
            Avm1Trees.DeepEquals(expected, actual).ShouldBeTrue($"{category}: {Path.GetFileName(path)}");
        }
    }
}
