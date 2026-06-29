using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record ItemsetsFile(
    [property: Avm1Property("IS")] Dictionary<string, Itemset> Itemsets);
