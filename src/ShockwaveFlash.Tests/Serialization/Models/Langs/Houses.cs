using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record HousesFile(
    [property: Avm1Property("H")] Houses Houses);

[Avm1Object]
public partial record Houses(
    [property: Avm1Property("h")] Dictionary<string, HouseEntry> Entries,
    [property: Avm1Property("m")] Avm1Object Maps,
    [property: Avm1Property("d")] Avm1Object Doors);

[Avm1Object]
public partial record HouseEntry(
    [property: Avm1Property("d")] string Description,
    [property: Avm1Property("n")] string Name);
