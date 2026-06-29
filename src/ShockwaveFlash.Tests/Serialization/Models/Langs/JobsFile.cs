using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record JobsFile(
    [property: Avm1Property("J")] Dictionary<string, Job> Jobs);
