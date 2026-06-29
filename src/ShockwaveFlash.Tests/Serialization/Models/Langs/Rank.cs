using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record Rank(
    [property: Avm1Property("i")] int Id,
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("o")] int Order);
