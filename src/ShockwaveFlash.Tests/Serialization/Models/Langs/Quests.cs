using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record QuestsFile(
    [property: Avm1Property("Q")] Quests Quests);

[Avm1Object]
public partial record Quests(
    [property: Avm1Property("q")] Avm1Object Categories,
    [property: Avm1Property("s")] Dictionary<string, Quest> Steps,
    [property: Avm1Property("o")] Avm1Object Optional,
    [property: Avm1Property("t")] Avm1Object Types);

[Avm1Object]
public partial record Quest(
    [property: Avm1Property("d")] string Description,
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("r")] Avm1Array Rewards,
    [property: Avm1Property("rbl")] Avm1Array RewardLabels);
