using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record MapsFile(
    [property: Avm1Property("MA")] MapData Maps);

[Avm1Object]
public partial record MapData(
    [property: Avm1Property("m")] Dictionary<string, MapEntry> Entries,
    [property: Avm1Property("sa")] Avm1Object SubAreas,
    [property: Avm1Property("sua")] Avm1Object SuperAreas,
    [property: Avm1Property("a")] Avm1Object Areas);

[Avm1Object]
public partial record MapEntry(
    [property: Avm1Property("ep")] int Expansion,
    [property: Avm1Property("p1")] string? Place1,
    [property: Avm1Property("p2")] string? Place2,
    [property: Avm1Property("sa")] int SubArea,
    [property: Avm1Property("x")] int X,
    [property: Avm1Property("y")] int Y);
