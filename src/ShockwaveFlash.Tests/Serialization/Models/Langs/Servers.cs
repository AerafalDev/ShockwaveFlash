using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record ServersFile(
    [property: Avm1Property("SR")] Dictionary<string, Server> Servers,
    [property: Avm1Property("SRP")] Dictionary<string, string> Populations,
    [property: Avm1Property("SRPW")] Dictionary<string, int> PopulationWeights,
    [property: Avm1Property("SRC")] Dictionary<string, ServerCommunity> Communities,
    [property: Avm1Property("SRVT")] Dictionary<string, ServerVote> Votes,
    [property: Avm1Property("SRVC")] Dictionary<string, string> VoteChoices);

[Avm1Object]
public partial record Server(
    [property: Avm1Property("c")] string Community,
    [property: Avm1Property("d")] string Description,
    [property: Avm1Property("date")] string Date,
    [property: Avm1Property("l")] string Language,
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("p")] string Population,
    [property: Avm1Property("rlng")] string[] Languages,
    [property: Avm1Property("t")] int Type);

[Avm1Object]
public partial record ServerCommunity(
    [property: Avm1Property("c")] string[] Countries,
    [property: Avm1Property("d")] bool Default,
    [property: Avm1Property("i")] int Id,
    [property: Avm1Property("n")] string Name);

[Avm1Object]
public partial record ServerVote(
    [property: Avm1Property("d")] string Description,
    [property: Avm1Property("l")] string Label);
