# AVM1 serialization guide

Map AVM1 global variables (the AS2 data inside a `DoAction` tag) to and from your own C# types.
The serializer is modeled on `System.Text.Json`: a converter core, a per-type `Avm1TypeInfo`, and two
interchangeable front-ends — a **source-generated context** (reflection-free, AOT-safe) and a
**reflection** fallback. Both funnel through the same converters, so they always agree.

- [Quick start](#quick-start)
- [The two modes](#the-two-modes)
- [What maps](#what-maps)
- [Construction](#construction)
- [Customizing members](#customizing-members)
- [Optional members, null, and defaults](#optional-members-null-and-defaults)
- [Numbers](#numbers)
- [Custom converters](#custom-converters)
- [Extension data](#extension-data)
- [Binding to globals](#binding-to-globals)
- [Options](#options)
- [Diagnostics](#diagnostics)

## Quick start

```csharp
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;

public record Emote(
    [property: Avm1Property("s")] string Shortcut,
    [property: Avm1Property("n")] string Name);

[Avm1Serializable(typeof(Emote), "EM")]   // Emote is bound to the global at globals.EM
public partial class LangContext : Avm1SerializerContext;
```

```csharp
var globals = tag.Evaluate(version);                 // an Avm1Object of all globals

Emote emote = LangContext.Default.Read<Emote>(globals)!;          // reads globals.EM
LangContext.Default.Write(globals, emote with { Name = "Hi" });   // writes globals.EM
```

`Avm1Serializer.Serialize(value, LangContext.Default.Emote)` / `Deserialize(tree, …)` convert a value
to and from an `Avm1Value` **directly**, with no global path.

## The two modes

| | Source-generated context | Reflection |
|---|---|---|
| Entry point | `MyContext.Default` | `Avm1Serializer` with default options |
| Value ⟷ tree | `Avm1Serializer.Serialize/Deserialize(value, ctx.T)` | `Avm1Serializer.Serialize/Deserialize<T>(value)` |
| Global ⟷ tree | `ctx.Read<T>/Write<T>(globals)` | `Avm1Serializer.ReadGlobal/WriteGlobal<T>(globals)` |
| Path source | `[Avm1Serializable(typeof(T), "EM")]` | `[Avm1Object("EM")]` on the type |
| AOT / trimming | safe | not safe (`[RequiresDynamicCode]`) |

A context is a `partial class : Avm1SerializerContext` with one or more `[Avm1Serializable]` attributes;
the generator fills in `Default`, the constructors, a typed `Avm1TypeInfo<T>` accessor per type, and the
`GetTypeInfo` dispatch. `MyContext.Default.Options` exposes its `Avm1SerializerOptions`; the context
resolves its registered types itself and falls back to reflection for everything else (collections,
scalars, types you did not register).

Reflection mode needs no context — call `Avm1Serializer` directly. It is the convenient default; the
context is the AOT-safe, allocation-light option.

## What maps

A member maps when its type is one of:

- **Scalars** — `string`, `bool`, every numeric type (`byte`/`sbyte`/`short`/`ushort`/`int`/`uint`/
  `long`/`ulong`/`float`/`double`/`decimal`), and `enum` (written as its numeric value). All numbers are
  widened to `double` in the AVM1 tree.
- **Nested objects** — any other `class`/`record`/`struct`. Its members are bound the same way; no
  marker attribute is needed.
- **Collections** — `T[]`, `List<T>` (or `IList<T>`/`IReadOnlyList<T>`/`IEnumerable<T>`), and
  `Dictionary<string, V>` (or `IDictionary`/`IReadOnlyDictionary`). Elements and values may be any
  supported type, so `int[][]` and `Dictionary<string, Dictionary<string, int[]>>` nest freely.
- **Pass-through** — `Avm1Value`, `Avm1Object`, `Avm1Array`: stored and restored verbatim, the escape
  hatch for irreducibly heterogeneous data.
- **Custom** — anything, via `[Avm1Converter]` (see [Custom converters](#custom-converters)).

Only `public` and `internal` members participate; `static`, `const`, and `[Avm1Ignore]` members are
skipped.

## Construction

On read the serializer builds the instance one of two ways:

- **Constructor binding** — a positional `record`, or a type with a single parameterized constructor:
  each parameter is matched to a member by name (case-insensitive) and supplied from the tree.
- **Object initializer** — a type with an accessible parameterless constructor (or a `struct`): the
  instance is created, then each settable member (`set` or `init`) is assigned.

```csharp
public record Job(int Group, string Name);                 // constructor binding

public class Settings { public string? Theme { get; init; } public bool Mute { get; init; } }
```

If a type has several constructors and the choice is ambiguous, mark one with **`[Avm1Constructor]`**:

```csharp
public class Box
{
    public Box(int a) { ... }
    [Avm1Constructor] public Box(int a, int b) { ... }   // this one is used
}
```

A type with no usable constructor reports [AVM1003](diagnostics.md#avm1003).

## Customizing members

| Attribute | Effect |
|-----------|--------|
| `[Avm1Property("k")]` | Override the AVM1 key (default: the member name). |
| `[Avm1Ignore]` | Exclude the member entirely. |
| `[Avm1Required]` | Throw if the key is missing on read, even for a type that would otherwise default. |
| `[Avm1PropertyOrder(n)]` | Set the write order (ascending; ties keep declaration order). |
| `[Avm1Converter(typeof(C))]` | Use a custom converter for this member (wins over everything). |
| `[Avm1ExtensionData]` | Capture unknown keys (see [Extension data](#extension-data)). |

```csharp
public record Quest(
    [property: Avm1Property("n")] string Name,
    [property: Avm1Required] int Level,
    [property: Avm1PropertyOrder(-1)] string Id);     // written first
```

## Optional members, null, and defaults

A **nullable** member (`int?`, `string?`, `Weapon?`, `List<int>?`, …) is optional: a `null` value is
omitted on write, and a missing key reads back as `null`.

A **non-nullable reference / nested / pass-through** member is required-on-read — a missing key throws
([AVM1004 is for duplicate keys](diagnostics.md); the throw is a runtime
`Avm1SerializationException`). A non-nullable **numeric/`bool`/`enum`** silently defaults to `0`/`false`
on a missing key unless you add `[Avm1Required]`. Collections default to empty.

Write behaviour for `null` is controlled by `Avm1SerializerOptions.DefaultIgnoreCondition`:

| `Avm1IgnoreCondition` | On write |
|-----------------------|----------|
| `WhenWritingNull` (default) | omit `null` members |
| `Never` | write `null` members as `Avm1Null` |
| `WhenWritingDefault` | omit members equal to their type's default |
| `Always` | omit every member |

## Numbers

`Avm1Number` wraps a `double`. `Avm1SerializerOptions.NumberHandling` (a `[Flags]` enum) tunes
conversion, mirroring how AVM1 itself coerces:

| Flag | Effect |
|------|--------|
| `Strict` (default) | numbers are numbers |
| `AllowReadingFromString` | read a number from an `Avm1String` (`"42"` → `42`) |
| `WriteAsString` | write numbers as strings |
| `AllowNamedFloatingPointLiterals` | read/write `NaN`/`Infinity`/`-Infinity` (real AVM1 values) |

## Custom converters

Subclass `Avm1Converter<T>` to own the read/write of a type:

```csharp
public sealed class CoordConverter : Avm1Converter<Coord>
{
    public override Coord Read(Avm1Value v, Avm1SerializerOptions o)
    {
        var parts = v.AsString.Split(',');
        return new Coord(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    public override Avm1Value Write(Coord c, Avm1SerializerOptions o) => new Avm1String($"{c.X},{c.Y}");
}
```

Attach it to a **type** or a **member**, or register it on the options. Resolution order (highest first):

1. `[Avm1Converter]` on the **member**.
2. a converter in `Options.Converters` (first whose `CanConvert` matches; `Avm1ConverterFactory`
   instances are asked to create one).
3. `[Avm1Converter]` on the **type**.
4. the built-in converter.

A type handled by a custom converter is **opaque**: it has no configurable properties and is read/written
wholly by the converter — the same rule `System.Text.Json` uses.

## Extension data

A `Dictionary<string, Avm1Value>` member marked `[Avm1ExtensionData]` is the catch-all: on read every key
not claimed by a declared member is collected into it; on write its entries are flattened back into the
object verbatim. A typed partial model plus an extension bag round-trips losslessly — ideal for the
heterogeneous langs globals (`monsters.g1..g10`, `quests.r`, …).

```csharp
public record Monster(
    [property: Avm1Property("n")] string Name,
    [property: Avm1ExtensionData] Dictionary<string, Avm1Value> Rest);
```

## Binding to globals

A SWF's globals are one big `Avm1Object` keyed by variable name. A type's **binding path** says which
variable (and, with a dotted path, which nested member) it occupies.

```csharp
[Avm1Serializable(typeof(Emotes), "EM")]          // globals.EM
[Avm1Serializable(typeof(HouseList), "H.h")]      // globals.H.h
[Avm1Serializable(typeof(Foo), Segments = new[] { "a.b", "c" })]   // escapes a key containing '.'
public partial class DofusLangContext : Avm1SerializerContext;
```

- `ctx.Read<T>(globals)` walks the path and deserializes the leaf (returns `default`/`null` if a segment
  is absent). `ctx.Write(globals, value)` serializes and stores it, creating intermediate `Avm1Object`s.
- In reflection mode the path comes from `[Avm1Object("EM")]` on the type, used by
  `Avm1Serializer.ReadGlobal/WriteGlobal<T>`.
- A type registered with no path maps the **whole** globals object (use `Serialize`/`Deserialize`, not
  `Read`/`Write`).
- **`TypeInfoPropertyName`** registers one type at several paths under distinct accessors; serialize a
  specific one with `Avm1Serializer.ReadGlobal/WriteGlobal(globals, value, ctx.Accessor)`.

```csharp
[Avm1Serializable(typeof(Coord), "p1", TypeInfoPropertyName = "First")]
[Avm1Serializable(typeof(Coord), "p2", TypeInfoPropertyName = "Second")]
public partial class C : Avm1SerializerContext;

Avm1Serializer.WriteGlobal(globals, coord, C.Default.First);   // globals.p1
```

## Options

`Avm1SerializerOptions` carries the cross-cutting settings:

```csharp
var options = new Avm1SerializerOptions
{
    DefaultIgnoreCondition = Avm1IgnoreCondition.Never,
    NumberHandling = Avm1NumberHandling.AllowReadingFromString,
    IncludeFields = true,
};
options.Converters.Add(new CoordConverter());

var tree = Avm1Serializer.Serialize(value, options);
```

`Avm1SerializerOptions.Default` is the shared reflection instance. A context exposes its own configured
`Options`. `TypeInfoResolver` selects the metadata source (a context, the reflection resolver, or
`Avm1TypeInfoResolver.Combine(MyContext.Default, new DefaultAvm1TypeInfoResolver())` to chain them).

## Diagnostics

When a registered type cannot be mapped the source generator reports an `AVM1xxx` error instead of
emitting broken code. See the [diagnostics catalog](diagnostics.md).
