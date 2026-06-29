using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object("A")]
public partial record Alignment(
    [property: Avm1Property("a")] Dictionary<string, AlignmentSide> Sides,
    [property: Avm1Property("o")] Dictionary<string, AlignmentOrder> Orders,
    [property: Avm1Property("jo")] Dictionary<string, bool[]> JoinRights,
    [property: Avm1Property("at")] Dictionary<string, bool[]> AttackRights,
    [property: Avm1Property("f")] Dictionary<string, AlignmentFeat> Feats,
    [property: Avm1Property("fe")] Dictionary<string, string> FeatEffects,
    [property: Avm1Property("b")] Dictionary<string, AlignmentBalance> Balances,
    [property: Avm1Property("g")] Dictionary<string, bool[]> GuildRights,
    [property: Avm1Property("s")] Dictionary<string, AlignmentSpecialization> Specializations);
