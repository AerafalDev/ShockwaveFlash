using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record PvpFile(
    [property: Avm1Property("PP")] Pvp Pvp);

[Avm1Object]
public partial record Pvp(
    [property: Avm1Property("hp")] int[] HonorPoints,
    [property: Avm1Property("maxdp")] int MaxDishonor,
    [property: Avm1Property("grds")] PvpGuard[][] Guards);

[Avm1Object]
public partial record PvpGuard(
    [property: Avm1Property("nc")] string NameMale,
    [property: Avm1Property("nl")] string NameFemale);
