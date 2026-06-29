using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record DialogFile(
    [property: Avm1Property("D")] Dialog Dialog);

[Avm1Object]
public partial record Dialog(
    [property: Avm1Property("q")] Dictionary<string, string> Questions,
    [property: Avm1Property("a")] Avm1Object Answers);
