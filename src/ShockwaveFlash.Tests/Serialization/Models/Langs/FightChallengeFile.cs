using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record FightChallengeFile(
    [property: Avm1Property("FC")] Dictionary<string, FightChallenge> Challenges);
