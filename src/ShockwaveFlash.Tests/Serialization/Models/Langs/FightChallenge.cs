using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record FightChallenge(
    [property: Avm1Property("d")] string Description,
    [property: Avm1Property("g")] int Group,
    [property: Avm1Property("n")] string Name);
