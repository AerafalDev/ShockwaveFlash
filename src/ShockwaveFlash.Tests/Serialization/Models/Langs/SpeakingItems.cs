using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record SpeakingItemsFile(
    [property: Avm1Property("SIM")] Dictionary<string, SpeakingItem> Messages,
    [property: Avm1Property("SIT")] Dictionary<string, Dictionary<string, int[]>> Triggers);

[Avm1Object]
public partial record SpeakingItem(
    [property: Avm1Property("l")] int Level,
    [property: Avm1Property("m")] string Message,
    [property: Avm1Property("p")] double Probability,
    [property: Avm1Property("r")] string? Restriction,
    [property: Avm1Property("s")] int State);
