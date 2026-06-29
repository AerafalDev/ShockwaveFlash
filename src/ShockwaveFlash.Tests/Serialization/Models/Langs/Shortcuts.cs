using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record ShortcutsFile(
    [property: Avm1Property("SST")] Dictionary<string, ShortcutType> Types,
    [property: Avm1Property("SSK")] Dictionary<string, Shortcut> Keys);

[Avm1Object]
public partial record ShortcutType(
    [property: Avm1Property("d")] string Description,
    [property: Avm1Property("i")] int Id);

[Avm1Object]
public partial record Shortcut(
    [property: Avm1Property("c")] int? Code,
    [property: Avm1Property("c2")] int? Code2,
    [property: Avm1Property("d")] string Description,
    [property: Avm1Property("k")] int? Key,
    [property: Avm1Property("k2")] int? Key2,
    [property: Avm1Property("o")] bool Down,
    [property: Avm1Property("o2")] bool? Down2,
    [property: Avm1Property("s")] string? Sprite,
    [property: Avm1Property("s2")] string? Sprite2);
