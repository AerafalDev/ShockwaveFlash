using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record TitlesFile(
    [property: Avm1Property("PT")] Dictionary<string, Title> Titles);
