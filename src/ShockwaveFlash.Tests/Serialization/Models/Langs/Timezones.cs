using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record TimezonesFile(
    [property: Avm1Property("T")] Timezones Timezones);

[Avm1Object]
public partial record Timezones(
    [property: Avm1Property("mspd")] int MillisecondsPerDay,
    [property: Avm1Property("hpd")] int HoursPerDay,
    [property: Avm1Property("z")] int Zone,
    [property: Avm1Property("tz")] Avm1Array ZoneTable,
    [property: Avm1Property("m")] Avm1Array MonthTable);
