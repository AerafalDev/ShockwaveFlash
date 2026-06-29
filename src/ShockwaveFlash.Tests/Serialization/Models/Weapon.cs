using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models;

[Avm1Object]
public partial record Weapon(
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("dmg")] int Damage);
