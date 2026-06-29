using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record Ride(
    [property: Avm1Property("c1")] string Color1,
    [property: Avm1Property("c2")] string Color2,
    [property: Avm1Property("c3")] string Color3,
    [property: Avm1Property("g")] string Group,
    [property: Avm1Property("n")] string Name);
