# ShockwaveFlash.Avm1

AVM1 (ActionScript 1/2 bytecode) support for [ShockwaveFlash](https://www.nuget.org/packages/ShockwaveFlash) — decode `DoAction` bytecode to a typed action model, evaluate data scripts to a typed value tree, edit, and write the bytecode back.

## Install

```sh
dotnet add package ShockwaveFlash.Avm1
```

## Read a data table

Linear data scripts (localization / config tables, common in older games) are replayed to their global variables:

```csharp
using ShockwaveFlash;
using ShockwaveFlash.Avm1;
using ShockwaveFlash.Tags.Action;

var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes("emotes.swf"));
var version = swf.Header.Version;
var tag = swf.Tags.OfType<DoActionTag>().First();

var globals = tag.Evaluate(version);                       // Avm1Object
var name = globals["EM"].AsObject["1"].AsObject["n"].AsString;
```

## Edit and write back

`Avm1Object` / `Avm1Array` are editable in place, and primitives convert implicitly:

```csharp
var globals = tag.Evaluate(version);
var emotes = globals["EM"].AsObject;

emotes["1"].AsObject["n"] = "New name";                                          // change
emotes["24"] = new Avm1Object { Members = { ["n"] = "New emote", ["s"] = "new" } }; // add

var output = swf.ReplaceTag(tag, tag.WithGlobals(globals, version)).Assemble();
File.WriteAllBytes("emotes.swf", output.ToArray());
```

## Work with raw actions

For surgical edits that preserve everything else byte-for-byte, go through the action list instead of the value tree:

```csharp
var actions = tag.DecodeActions(version);          // IReadOnlyList<Action>
// ... inspect or modify actions ...
var newTag = tag.WithActions(actions, version);    // re-encoded; byte-identical when unchanged
```

## Value model

`Evaluate` returns an `Avm1Object` whose members are `Avm1Value`s: `Avm1String`, `Avm1Number`, `Avm1Boolean`, `Avm1Null`, `Avm1Undefined`, `Avm1Object`, `Avm1Array`. Use `AsObject` / `AsArray` / `AsString` / `AsNumber` / `AsBoolean` to read them.

## Scope

- The decoder and encoder cover the documented AVM1 opcode set (SWF 1–7) and round-trip **byte-for-byte**.
- `Avm1Machine` is a focused evaluator for **linear data scripts** — no control-flow or function execution. Opcodes outside the supported subset are collected in `UnsupportedOpcodes`; pass `strict: true` to throw `Avm1UnsupportedActionException` instead of skipping them.

## License

[MIT](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE) © Aerafal
