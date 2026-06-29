using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record RideAbility(
    [property: Avm1Property("d")] string Description,
    [property: Avm1Property("e")] string Effect,
    [property: Avm1Property("n")] string Name);
