using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Tests.Models;

[Avm1Object]
public partial record Nested(
    [property: Avm1Property("flags")] Dictionary<string, bool[]> Flags,
    [property: Avm1Property("grid")] int[][] Grid,
    [property: Avm1Property("groups")] Dictionary<string, Dictionary<string, int>> Groups,
    [property: Avm1Property("raw")] Avm1Array Raw,
    [property: Avm1Property("opt")] string? Opt);
