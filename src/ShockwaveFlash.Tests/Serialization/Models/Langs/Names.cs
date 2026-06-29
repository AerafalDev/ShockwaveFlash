using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record NamesFile(
    [property: Avm1Property("NF")] Names Names);

[Avm1Object]
public partial record Names(
    [property: Avm1Property("n")] Dictionary<string, string> First,
    [property: Avm1Property("f")] Dictionary<string, string> Last);
