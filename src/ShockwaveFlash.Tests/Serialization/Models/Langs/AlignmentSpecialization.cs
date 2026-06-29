using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record AlignmentSpecialization(
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("d")] string Description,
    [property: Avm1Property("o")] int Order,
    [property: Avm1Property("av")] int Alignment,
    [property: Avm1Property("f")] Avm1Array Feats);
