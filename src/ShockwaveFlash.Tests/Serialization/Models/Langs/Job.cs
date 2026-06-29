using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record Job(
    [property: Avm1Property("g")] int Group,
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("s")] int Specialization);
