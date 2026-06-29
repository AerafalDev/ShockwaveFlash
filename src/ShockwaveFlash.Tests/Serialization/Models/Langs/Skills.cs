using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record SkillsFile(
    [property: Avm1Property("SK")] Dictionary<string, Skill> Skills);

[Avm1Object]
public partial record Skill(
    [property: Avm1Property("c")] string? Criterion,
    [property: Avm1Property("cl")] int[]? CraftList,
    [property: Avm1Property("d")] string Description,
    [property: Avm1Property("f")] int? Function,
    [property: Avm1Property("i")] int? Interaction,
    [property: Avm1Property("io")] int InteractiveObject,
    [property: Avm1Property("j")] int Job);
