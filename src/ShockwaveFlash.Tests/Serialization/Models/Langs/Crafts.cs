using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record CraftsFile(
    [property: Avm1Property("CR")] Dictionary<string, int[][]> Crafts);
