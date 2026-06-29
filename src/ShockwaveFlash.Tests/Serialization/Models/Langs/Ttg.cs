using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record TtgFile(
    [property: Avm1Property("TTG")] Ttg Ttg);

[Avm1Object]
public partial record Ttg(
    [property: Avm1Property("c")] Dictionary<string, TtgEntry> Entries);

[Avm1Object]
public partial record TtgEntry(
    [property: Avm1Property("e")] int E,
    [property: Avm1Property("i")] int I,
    [property: Avm1Property("o")] int O,
    [property: Avm1Property("v")] int V);
