using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record State(
    [property: Avm1Property("d")] bool Defensive,
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("p")] int Priority,
    [property: Avm1Property("s")] string Sprite);
