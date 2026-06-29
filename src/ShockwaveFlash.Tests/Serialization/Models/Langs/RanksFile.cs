using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record RanksFile(
    [property: Avm1Property("R")] Dictionary<string, Rank> Ranks);
