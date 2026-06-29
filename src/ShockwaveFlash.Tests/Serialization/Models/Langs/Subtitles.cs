using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization;

namespace ShockwaveFlash.Tests.Models.Langs;

[Avm1Object]
public partial record SubtitlesFile(
    [property: Avm1Property("SUB")] Dictionary<string, Dictionary<string, string>> Subtitles);
