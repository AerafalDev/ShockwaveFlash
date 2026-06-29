using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models;

[Avm1Object("player")]
public partial record Player(
    [property: Avm1Property("name")] string Name,
    [property: Avm1Property("score")] int Score,
    Rarity Rank,
    Weapon Equipped,
    Weapon? Sidearm,
    List<int> Inventory,
    string[] Tags,
    Dictionary<string, double> Stats);
