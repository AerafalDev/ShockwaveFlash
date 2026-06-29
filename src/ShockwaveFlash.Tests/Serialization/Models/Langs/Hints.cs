using ShockwaveFlash.Avm1.Serialization;
using System.Collections.Generic;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record HintsFile(
    [property: Avm1Property("HIC")] Dictionary<string, HintCategory> Categories,
    [property: Avm1Property("HI")] HintLink[] Links,
    [property: Avm1Property("HIN")] HintZone[] Zones);

[Avm1Object]
public partial record HintCategory(
    [property: Avm1Property("c")] string Color,
    [property: Avm1Property("n")] string Name);

[Avm1Object]
public partial record HintLink(
    [property: Avm1Property("c")] int Category,
    [property: Avm1Property("g")] int Gfx,
    [property: Avm1Property("m")] int Map,
    [property: Avm1Property("n")] string Name);

[Avm1Object]
public partial record HintZone(
    [property: Avm1Property("h")] HintLink[] Hints,
    [property: Avm1Property("x")] int X,
    [property: Avm1Property("y")] int Y);
