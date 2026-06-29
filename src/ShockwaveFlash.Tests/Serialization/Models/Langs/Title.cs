using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record Title(
    [property: Avm1Property("c")] int Category,
    [property: Avm1Property("pt")] int Points,
    [property: Avm1Property("t")] string Text);
