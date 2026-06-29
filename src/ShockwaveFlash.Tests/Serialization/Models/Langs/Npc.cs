using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record NpcFile(
    [property: Avm1Property("N")] Npc Npc);

[Avm1Object]
public partial record Npc(
    [property: Avm1Property("d")] Dictionary<string, NpcEntry> Dialogs,
    [property: Avm1Property("a")] Dictionary<string, string> Actions);

[Avm1Object]
public partial record NpcEntry(
    [property: Avm1Property("a")] int[]? Answers,
    [property: Avm1Property("n")] string Name);
