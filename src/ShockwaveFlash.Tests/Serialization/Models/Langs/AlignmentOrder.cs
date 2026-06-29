using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record AlignmentOrder(
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("a")] int Side);
