using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record AlignmentSide(
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("c")] bool Choosable);
