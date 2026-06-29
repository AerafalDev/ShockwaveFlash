# ShockwaveFlash.Avm1

[![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.Avm1.svg)](https://www.nuget.org/packages/ShockwaveFlash.Avm1)
[![Downloads](https://img.shields.io/nuget/dt/ShockwaveFlash.Avm1.svg)](https://www.nuget.org/packages/ShockwaveFlash.Avm1)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE)

AVM1 (ActionScript 1/2 bytecode) support for
[**ShockwaveFlash**](https://www.nuget.org/packages/ShockwaveFlash). Disassemble `DoAction` bytecode to a
mutable action tree (and back, losslessly), dump it as **p-code** or best-effort **AS2**, evaluate linear
data scripts to a typed value tree you can edit and write back — or map them to your own records with the
bundled, `System.Text.Json`-style source generator.

```sh
dotnet add package ShockwaveFlash.Avm1
```

```csharp
using ShockwaveFlash.Avm1.Types;

Avm1Object globals = tag.Evaluate(version);                 // replay the data script
string name = globals["EM"].AsObject["1"].AsObject["n"].AsString;
```

```csharp
// ...or map a global to your own type, reflection-free
[Avm1Serializable(typeof(Emote), "EM")]
public partial class LangContext : Avm1SerializerContext;

Emote emote = LangContext.Default.Read<Emote>(globals)!;
```

## Documentation

- **[AVM1 bytecode & value tree →](https://aerafaldev.github.io/ShockwaveFlash/docs/avm1)**
- **[Serialization →](https://aerafaldev.github.io/ShockwaveFlash/docs/serialization)**

---

Part of the [ShockwaveFlash](https://github.com/AerafalDev/ShockwaveFlash) project ·
[MIT](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE) © Aerafal
