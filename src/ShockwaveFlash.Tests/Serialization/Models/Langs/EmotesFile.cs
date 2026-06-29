using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record EmotesFile(
    [property: Avm1Property("EM")] Dictionary<string, Emote> Emotes);
