using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record DungeonsFile(
    [property: Avm1Property("DU")] Dictionary<string, Dungeon> Dungeons);

[Avm1Object]
public partial record Dungeon(
    [property: Avm1Property("m")] Dictionary<string, DungeonRoom> Rooms,
    [property: Avm1Property("n")] string Name);

[Avm1Object]
public partial record DungeonRoom(
    [property: Avm1Property("i")] int? Icon,
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("x")] int X,
    [property: Avm1Property("y")] int Y,
    [property: Avm1Property("z")] int Z);
