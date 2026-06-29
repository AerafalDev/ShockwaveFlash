using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record AlignmentBalance(
    [property: Avm1Property("s")] int Start,
    [property: Avm1Property("e")] int End,
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("d")] string Description);
