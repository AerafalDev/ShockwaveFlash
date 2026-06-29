using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record StatesFile(
    [property: Avm1Property("ST")] Dictionary<string, State> States);
