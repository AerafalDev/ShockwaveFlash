using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record EffectsFile(
    [property: Avm1Property("E")] Dictionary<string, Effect> Effects,
    [property: Avm1Property("EDMG")] Dictionary<string, int> Damages);

[Avm1Object]
public partial record Effect(
    [property: Avm1Property("c")] int Characteristic,
    [property: Avm1Property("d")] string Description,
    [property: Avm1Property("e")] string? Extra,
    [property: Avm1Property("j")] bool? Jet,
    [property: Avm1Property("o")] string Operator,
    [property: Avm1Property("p")] int Priority,
    [property: Avm1Property("t")] bool? Trap);
