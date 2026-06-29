using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record InteractiveObjectsFile(
    [property: Avm1Property("IO")] InteractiveObjects InteractiveObjects);

[Avm1Object]
public partial record InteractiveObjects(
    [property: Avm1Property("g")] Dictionary<string, int> Gfx,
    [property: Avm1Property("d")] Dictionary<string, InteractiveObject> Definitions);

[Avm1Object]
public partial record InteractiveObject(
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("sk")] int[] Skills,
    [property: Avm1Property("t")] int Type);
