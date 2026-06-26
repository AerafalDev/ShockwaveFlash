# ShockwaveFlash.Avm1

AVM1 (ActionScript 1/2 bytecode) support for [ShockwaveFlash](https://www.nuget.org/packages/ShockwaveFlash) — decode `DoAction` bytecode to a typed action model, evaluate data scripts to a value tree, edit, and write the bytecode back.

## Install

```sh
dotnet add package ShockwaveFlash.Avm1
```

## Decode / encode actions

```csharp
using ShockwaveFlash;
using ShockwaveFlash.Avm1;
using ShockwaveFlash.Tags.Action;

var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes("script.swf"));
var version = swf.Header.Version;
var tag = swf.Tags.OfType<DoActionTag>().First();

var actions = Action.DecodeCollection(tag.Data, version);   // IReadOnlyList<Action>
var bytes   = Action.EncodeCollection(actions, version);    // byte-exact mirror
```

## Evaluate a data script

Linear data scripts (e.g. localization / config tables) are replayed to their global variables:

```csharp
var globals = Avm1Machine.Run(tag.Data, version);   // Avm1Object
var name = globals["EM"].AsObject["1"].AsObject["n"].AsString;
```

## Edit and write back

```csharp
var globals = tag.Evaluate(version);
globals["EM"].AsObject["1"].AsObject["n"] = "New name";

var newTag = tag.WithGlobals(globals, version);     // re-emit AVM1 bytecode
var output = swf.ReplaceTag(tag, newTag).Assemble();
File.WriteAllBytes("script.swf", output.ToArray());
```

## Scope

The decoder/encoder cover the documented AVM1 opcode set (SWF 1–7) and round-trip byte-for-byte. `Avm1Machine` is a focused evaluator for **linear data scripts** (no control flow or function bodies); pass `strict: true` to fail fast when an unsupported opcode is encountered instead of skipping it.

## License

[MIT](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE) © Aerafal
