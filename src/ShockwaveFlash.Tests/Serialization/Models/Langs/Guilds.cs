using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record GuildsFile(
    [property: Avm1Property("GU")] Guilds Guilds);

[Avm1Object]
public partial record Guilds(
    [property: Avm1Property("b")] GuildBonus Bonus);

[Avm1Object]
public partial record GuildBonus(
    [property: Avm1Property("sm")] int StrengthMax,
    [property: Avm1Property("xm")] int XpMax,
    [property: Avm1Property("cm")] int ChanceMax,
    [property: Avm1Property("pm")] int ProspectingMax,
    [property: Avm1Property("wm")] int WisdomMax,
    [property: Avm1Property("s")] int[][] Strength,
    [property: Avm1Property("x")] int[][] Xp,
    [property: Avm1Property("c")] int[][] Chance,
    [property: Avm1Property("p")] int[][] Prospecting,
    [property: Avm1Property("w")] int[][] Wisdom);
