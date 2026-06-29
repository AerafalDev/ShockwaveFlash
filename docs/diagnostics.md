# AVM1 serializer diagnostics

The `ShockwaveFlash.SourceGenerators` source generator emits an `IAvm1Serializable<TSelf>`
implementation for every type annotated with `[Avm1Object]`. When a type cannot be mapped it
reports one of the diagnostics below instead of producing broken code.

All diagnostics are `Error` severity (generation is skipped for the offending type) and belong to
the `Usage` category. The IDs are a permanent contract: suppress one with
`#pragma warning disable AVM1002`, an `.editorconfig` entry
(`dotnet_diagnostic.AVM1002.severity = none`), or `<NoWarn>AVM1002</NoWarn>`.

| ID | Title |
|----|-------|
| [AVM1001](#avm1001) | Type must be partial |
| [AVM1002](#avm1002) | Unsupported member type |
| [AVM1003](#avm1003) | No usable constructor |
| [AVM1004](#avm1004) | Duplicate member key |
| [AVM1005](#avm1005) | Containing type must be partial |
| [AVM1006](#avm1006) | Unsupported declaration |

## Supported member types

A member maps when its type is one of:

- **Scalars** — `string`, `bool`, any numeric type (`byte`/`int`/`long`/`float`/`double`/`decimal`/…),
  and `enum` (stored as its numeric value). Numbers are widened to `double` in the AVM1 tree.
- **Nested objects** — another type annotated with `[Avm1Object]`.
- **Collections** — `T[]`, `List<T>` (or `IList<T>`/`IReadOnlyList<T>`/`IEnumerable<T>`), and
  `Dictionary<string, V>` (or the read-only/interface variants). Elements/values may themselves be
  any supported type, so nested containers such as `int[][]` or
  `Dictionary<string, Dictionary<string, int[]>>` are allowed.
- **Pass-through** — `Avm1Value`, `Avm1Object` or `Avm1Array`. The value is stored and restored
  verbatim, which is the escape hatch for irreducibly heterogeneous data (unions, variable-length
  tuples, …).

Nullable members (`int?`, `string?`, `Weapon?`, `List<int>?`, …) are optional: a `null` value is
**omitted** from the object on write, and a missing key reads back as `null`.

Mark a member with `[Avm1Ignore]` to exclude it. Only `public` and `internal` members participate.

## AVM1001

**Type must be partial.**

> Type '{0}' is annotated with [Avm1Object] but is not declared 'partial'; the generator cannot add
> the IAvm1Serializable implementation.

The generator adds `ToAvm1Object`/`FromAvm1Object` in a second partial declaration, so the type has
to be `partial`.

```csharp
// ❌ AVM1001
[Avm1Object]
public record Emote(string Shortcut, string Name);

// ✅
[Avm1Object]
public partial record Emote(string Shortcut, string Name);
```

## AVM1002

**Unsupported member type.**

> Member '{0}.{1}' has type '{2}' which the AVM1 serializer cannot map; mark it with [Avm1Ignore] or
> use a supported type.

The member's type is not one of the [supported member types](#supported-member-types) — for
example `object`, `DateTime`, a tuple, or a `Dictionary<int, …>` (only `string` keys are allowed).

Fixes:

- Use a supported type, or model the value as a nested `[Avm1Object]` type.
- For genuinely dynamic data (a key that holds different shapes per entry, a heterogeneous array),
  type the member as `Avm1Value`, `Avm1Array` or `Avm1Object` to round-trip it verbatim.
- Mark it `[Avm1Ignore]` if it should not be (de)serialized.

```csharp
[Avm1Object]
public partial record Quest(
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("r")] Avm1Array Rewards); // heterogeneous → pass-through
```

## AVM1003

**No usable constructor.**

> Type '{0}' has no accessible parameterless constructor and no single constructor whose parameters
> all match (de)serialized members.

`FromAvm1Object` builds the instance either through a public parameterless constructor (then sets
the `init`/`set` members) or by binding a single constructor's parameters to members by name. A type
with only a multi-parameter constructor whose parameters do not all map to members cannot be built.

```csharp
// ✅ positional record: the primary constructor binds to the members
[Avm1Object]
public partial record Job(int Group, string Name, int Specialization);

// ✅ mutable type: parameterless ctor + init members
[Avm1Object]
public partial class Settings
{
    public string? Theme { get; init; }
    public bool Mute { get; init; }
}
```

## AVM1004

**Duplicate member key.**

> Members '{1}' and '{2}' on type '{0}' both map to the AVM1 key '{3}'; keys must be unique.

Two members resolve to the same AVM1 key — usually because `[Avm1Property("…")]` collides with
another member's name or attribute.

```csharp
// ❌ AVM1004: both map to "n"
[Avm1Object]
public partial record Bad(
    [property: Avm1Property("n")] string Name,
    string N);

// ✅
[Avm1Object]
public partial record Good(
    [property: Avm1Property("n")] string Name,
    [property: Avm1Property("note")] string N);
```

## AVM1005

**Containing type must be partial.**

> Type '{0}' is nested in '{1}' which is not declared 'partial'; every containing type must be
> partial for generation to succeed.

A nested `[Avm1Object]` type is emitted inside its containing type(s), so each enclosing type must
also be `partial`.

```csharp
// ✅
public partial class Catalog
{
    [Avm1Object]
    public partial record Entry(string Name);
}
```

## AVM1006

**Unsupported declaration.**

> Type '{0}' cannot be made AVM1 serializable; generic types and ref-like types are not supported.

`[Avm1Object]` cannot be applied to a generic type (`Foo<T>`) or a `ref struct`. Use a non-generic,
non-ref type.
