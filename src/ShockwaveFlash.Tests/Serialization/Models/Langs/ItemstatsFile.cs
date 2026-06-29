using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record ItemstatsFile(
    [property: Avm1Property("ISTA")] Dictionary<string, string> Stats);
