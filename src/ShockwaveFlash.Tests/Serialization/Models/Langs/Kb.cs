using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record KbFile(
    [property: Avm1Property("KBC")] Dictionary<string, KbCategory> Categories,
    [property: Avm1Property("KBA")] Dictionary<string, KbAction> Actions,
    [property: Avm1Property("KBT")] Dictionary<string, KbType> Types,
    [property: Avm1Property("KBD")] Dictionary<string, KbDescription> Descriptions);

[Avm1Object]
public partial record KbCategory(
    [property: Avm1Property("ep")] int Expansion,
    [property: Avm1Property("i")] int Id,
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("o")] int Order);

[Avm1Object]
public partial record KbAction(
    [property: Avm1Property("a")] string Action,
    [property: Avm1Property("c")] int Category,
    [property: Avm1Property("ep")] int Expansion,
    [property: Avm1Property("i")] int Id,
    [property: Avm1Property("k")] string[] Keys,
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("o")] int Order);

[Avm1Object]
public partial record KbType(
    [property: Avm1Property("c")] string Category,
    [property: Avm1Property("i")] int Id,
    [property: Avm1Property("l")] int Level);

[Avm1Object]
public partial record KbDescription(
    [property: Avm1Property("d")] int D,
    [property: Avm1Property("t")] int T,
    [property: Avm1Property("v")] Avm1Value V);
