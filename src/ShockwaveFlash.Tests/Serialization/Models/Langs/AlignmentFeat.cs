using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record AlignmentFeat(
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("g")] int Group,
    [property: Avm1Property("e")] int Effect);
