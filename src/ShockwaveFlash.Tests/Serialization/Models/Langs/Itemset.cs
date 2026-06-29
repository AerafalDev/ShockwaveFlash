using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record Itemset(
    [property: Avm1Property("i")] int[] Items,
    [property: Avm1Property("n")] string Name);
