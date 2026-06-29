using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record RidesFile(
    [property: Avm1Property("RI")] Dictionary<string, Ride> Rides,
    [property: Avm1Property("RIA")] Dictionary<string, RideAbility> Abilities);
