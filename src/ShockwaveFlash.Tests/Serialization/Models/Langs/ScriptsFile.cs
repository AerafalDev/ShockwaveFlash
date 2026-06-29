using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record ScriptsFile(
    [property: Avm1Property("SCR")] Dictionary<string, string> Scripts);
