using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record MonstersFile(
    [property: Avm1Property("MSR")] Dictionary<string, MonsterName> Super,
    [property: Avm1Property("MR")] Dictionary<string, MonsterName> Races,
    [property: Avm1Property("M")] Dictionary<string, Monster> Monsters);

[Avm1Object]
public partial record MonsterName(
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("s")] string Sprite);

[Avm1Object]
public partial record Monster(
    [property: Avm1Property("g1")] Avm1Object? Grade1,
    [property: Avm1Property("g2")] Avm1Object? Grade2,
    [property: Avm1Property("g3")] Avm1Object? Grade3,
    [property: Avm1Property("g4")] Avm1Object? Grade4,
    [property: Avm1Property("g5")] Avm1Object? Grade5,
    [property: Avm1Property("g6")] Avm1Object? Grade6,
    [property: Avm1Property("g7")] Avm1Object? Grade7,
    [property: Avm1Property("g8")] Avm1Object? Grade8,
    [property: Avm1Property("g9")] Avm1Object? Grade9,
    [property: Avm1Property("g10")] Avm1Object? Grade10,
    [property: Avm1Property("s")] bool Summonable,
    [property: Avm1Property("d")] bool Capturable,
    [property: Avm1Property("k")] bool Boss,
    [property: Avm1Property("a")] int Race,
    [property: Avm1Property("b")] int Super,
    [property: Avm1Property("g")] int Gfx,
    [property: Avm1Property("nn")] string ShortName,
    [property: Avm1Property("n")] string Name);
