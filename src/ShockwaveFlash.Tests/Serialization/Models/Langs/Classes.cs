using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record ClassesFile(
    [property: Avm1Property("G")] Dictionary<string, ClassData> Classes);

[Avm1Object]
public partial record ClassData(
    [property: Avm1Property("b10")] int[][] Boost10,
    [property: Avm1Property("b11")] int[][] Boost11,
    [property: Avm1Property("b12")] int[][] Boost12,
    [property: Avm1Property("b13")] int[][] Boost13,
    [property: Avm1Property("b14")] int[][] Boost14,
    [property: Avm1Property("b15")] int[][] Boost15,
    [property: Avm1Property("cc")] Avm1Value Spells,
    [property: Avm1Property("d")] string Description,
    [property: Avm1Property("di")] bool Disabled,
    [property: Avm1Property("ep")] int Expansion,
    [property: Avm1Property("ln")] string LongName,
    [property: Avm1Property("pd")] string MaleDescription,
    [property: Avm1Property("pt")] string FemaleDescription,
    [property: Avm1Property("s")] int[] Spec,
    [property: Avm1Property("sd")] string ShortDescription,
    [property: Avm1Property("sn")] string ShortName);
