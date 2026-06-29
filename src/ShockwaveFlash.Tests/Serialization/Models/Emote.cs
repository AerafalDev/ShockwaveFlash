using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models;

[Avm1Object]
public partial record Emote(
    [property: Avm1Property("s")] string Shortcut,
    [property: Avm1Property("n")] string Name);
