using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Tests.Models;

namespace ShockwaveFlash.Tests.Serialization;

[Avm1Serializable(typeof(Player), "player")]
[Avm1Serializable(typeof(Weapon))]
[Avm1Serializable(typeof(Nested))]
[Avm1Serializable(typeof(Emote))]
public partial class TestModelsContext : Avm1SerializerContext;
