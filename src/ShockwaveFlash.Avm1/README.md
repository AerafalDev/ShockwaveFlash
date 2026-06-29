# ShockwaveFlash.Avm1

[![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.Avm1.svg)](https://www.nuget.org/packages/ShockwaveFlash.Avm1)
[![Downloads](https://img.shields.io/nuget/dt/ShockwaveFlash.Avm1.svg)](https://www.nuget.org/packages/ShockwaveFlash.Avm1)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE)

AVM1 (ActionScript 1/2 bytecode) support for [**ShockwaveFlash**](https://www.nuget.org/packages/ShockwaveFlash). Disassemble `DoAction` bytecode to a mutable, strongly-typed action tree and assemble it back losslessly; dump it as **p-code** or best-effort **AS2**; and evaluate linear data scripts to a typed value tree you can edit and write back — or map them to your own records with the bundled source generator.

## Install

```sh
dotnet add package ShockwaveFlash.Avm1
```

## Disassemble to p-code or AS2

```csharp
using ShockwaveFlash;
using ShockwaveFlash.Avm1.Text;
using ShockwaveFlash.Tags.Action;

var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes("movie.swf"));
var version = swf.Header.Version;
var tag = swf.Tags.OfType<DoActionTag>().First();

string pcode = Avm1Disassembler.Disassemble(tag.Data, version, Avm1DisassemblyKind.Pcode);
string as2 = Avm1Disassembler.Disassemble(tag.Data, version, Avm1DisassemblyKind.As2);
```

**P-code** is a faithful, complete listing — every opcode with its operands, constant-pool references resolved, and function/with/try bodies decoded and indented:

```text
ConstantPool "VERSION", "DU", "Object", "n", "m", ...
Push "VERSION"
Push 1258
SetVariable
Push "DU"
Push 0
Push "Object"
NewObject
SetVariable
```

**AS2** is a best-effort reconstruction over the linear subset (push literals, get/set variable and member, object/array literals, operators, `new`/calls, function bodies). It is *not* a full decompiler: control flow and registers holding complex expressions fall back to their p-code line.

```as2
VERSION = 1258;
DU = new Object();
DU[1] = {n: "Donjon Bouftou", m: new Object()};
DU[1].m[2073] = {x: 0, y: 0, z: 0, n: "Salle 1", i: 900};
```

## Evaluate a data table

Linear data scripts (localization / config tables, common in older games) are replayed to their global variables, with version-accurate value coercion:

```csharp
using ShockwaveFlash.Avm1.Types;

var version = swf.Header.Version;
var tag = swf.Tags.OfType<DoActionTag>().First();

Avm1Object globals = tag.Evaluate(version);
string name = globals["EM"].AsObject["1"].AsObject["n"].AsString;
```

## Edit & write back

`Avm1Object` / `Avm1Array` are editable in place, and primitives convert implicitly:

```csharp
var globals = tag.Evaluate(version);
var emotes = globals["EM"].AsObject;

emotes["1"].AsObject["n"] = "New name";                                             // change
emotes["24"] = new Avm1Object { Members = { ["n"] = "New emote", ["s"] = "new" } }; // add

var output = swf.ReplaceTag(tag, tag.WithGlobals(globals, version)).Assemble();
File.WriteAllBytes("emotes.swf", output.ToArray());
```

## Typed models (source generator)

Instead of walking the value tree by string keys, map globals to your own records. Register the types
on a `partial` **serializer context** and the bundled source generator emits a reflection-free,
AOT-friendly resolver (no extra package reference required):

```csharp
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;

public record Emote(
    [property: Avm1Property("s")] string Shortcut,
    [property: Avm1Property("n")] string Name);

public record Alignment(
    [property: Avm1Property("a")] Dictionary<string, AlignmentSide> Sides,
    [property: Avm1Property("fe")] Dictionary<string, string> FeatEffects);

[Avm1Serializable(typeof(Emote))]
[Avm1Serializable(typeof(Alignment), "A")]   // bound to the global at globals.A
public partial class LangContext : Avm1SerializerContext;
```

`LangContext.Default` is the singleton resolver. A global that is a fixed-shape object reads and writes
through its binding path:

```csharp
var globals = tag.Evaluate(version);

Alignment alignment = LangContext.Default.Read<Alignment>(globals)!;   // reads globals.A
LangContext.Default.Write(globals, alignment);                         // writes globals.A
```

`EM` is an object keyed by id, so deserialize it as a map, edit, and serialize it back:

```csharp
var options = LangContext.Default.Options;

var emotes = Avm1Serializer.Deserialize<Dictionary<string, Emote>>(globals["EM"].AsObject, options);
emotes["1"] = emotes["1"] with { Name = "New name" };
emotes["24"] = new Emote("new", "New emote");
globals["EM"] = Avm1Serializer.Serialize(emotes, options);

var output = swf.ReplaceTag(tag, tag.WithGlobals(globals, version)).Assemble();
```

Without a context the reflection serializer works the same — annotate the type's path with
`[Avm1Object("A")]` and call `Avm1Serializer.ReadGlobal<Alignment>(globals)` /
`WriteGlobal(globals, alignment)`, or plain `Avm1Serializer.Serialize`/`Deserialize` for a value with
no global binding.

Customize members with `[Avm1Property("k")]` (key override), `[Avm1Ignore]`, `[Avm1Required]`,
`[Avm1PropertyOrder(n)]`, `[Avm1Converter(typeof(C))]` (a custom `Avm1Converter<T>`, on a member or a
type), and `[Avm1ExtensionData]` (a `Dictionary<string, Avm1Value>` capturing unknown keys); pick a
constructor with `[Avm1Constructor]`. `Avm1SerializerOptions` tunes `NumberHandling`,
`DefaultIgnoreCondition`, and custom `Converters`. The
**[serialization guide](https://github.com/AerafalDev/ShockwaveFlash/blob/main/references/avm1-serialization.md)**
walks through every case; mapping problems surface as
[`AVM1xxx` diagnostics](https://github.com/AerafalDev/ShockwaveFlash/blob/main/references/diagnostics.md).

Supported member types: scalars (`string`, `bool`, any numeric — widened to `double` — and `enum`),
nested objects (any record/class/struct), collections (`T[]`, `List<T>`, `Dictionary<string, V>`) that
nest to any depth (`int[][]`, `Dictionary<string, Dictionary<string, int[]>>`, …), custom
`[Avm1Converter]` members, and the verbatim escape hatch `Avm1Value` / `Avm1Array` / `Avm1Object` for
irreducibly heterogeneous fields (unions, tuples). Nullable members are optional — a `null` is omitted
on write and a missing key reads back as `null`.

## Raw actions

For surgical edits that preserve everything else, go through the mutable action tree instead of the value tree. Every action is a mutable class with settable properties; function/with/try bodies are owned, so editing one action never corrupts another:

```csharp
var actions = tag.DecodeActions(version);       // IReadOnlyList<Action>
// ... inspect or mutate actions in place ...
var newTag = tag.WithActions(actions, version); // re-encoded; byte-identical when unchanged
```

`Action.DecodeCollection(bytes, version)` decodes leniently by default — unknown opcodes are kept as `ActionUnknown`, an unknown push type stops that push, and a length mismatch is tolerated. Pass `strict: true` to promote all of these to a typed `SwfFormatException` (and to reject malformed SWF6+ UTF-8 strings).

## What it is — and isn't

- **Faithful disassembler + assembler.** The documented opcode set (SWF 1–7) round-trips **byte-for-byte**; the action tree is mutable, and unknown tags/opcodes are preserved.
- **Version-accurate evaluator.** `Avm1Machine` runs the pure (branch-free) operators — arithmetic, bitwise, comparison, logic, string, type and stack ops, plus a register file — over value coercion ported faithfully from Flash (number formatting included).
- **Linear only.** The evaluator is a data-script interpreter: no control flow, no function calls, no host/display objects. Opcodes outside that subset are recorded in `UnsupportedOpcodes` (or throw `Avm1UnsupportedActionException` when `strict`). The AS2 listing is best-effort, not a decompiler.
- **AVM2 (`DoABC`)** is out of scope — its bytecode is kept raw by the core package.

---

Part of the [ShockwaveFlash](https://github.com/AerafalDev/ShockwaveFlash) project · [MIT](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE) © Aerafal
