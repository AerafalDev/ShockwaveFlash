using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record AudioFile(
    [property: Avm1Property("AUMC")] Dictionary<string, int> MusicByCombat,
    [property: Avm1Property("AUM")] Dictionary<string, AudioMusic> Music,
    [property: Avm1Property("AUEC")] Dictionary<string, int> EffectByCombat);

[Avm1Object]
public partial record AudioMusic(
    [property: Avm1Property("f")] string File,
    [property: Avm1Property("l")] bool Loop,
    [property: Avm1Property("o")] int Order,
    [property: Avm1Property("s")] bool Stream,
    [property: Avm1Property("v")] int Volume);
