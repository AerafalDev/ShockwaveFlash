# AVM1 serialization architecture (design — targeting 1.6)

> Status: **draft**. This generalizes the current `[Avm1Object]` source generator into a
> converter-centric serializer modeled on the internal architecture of `System.Text.Json` (STJ),
> adapted to the fact that AVM1 is an in-memory value **tree** (`Avm1Value`), not a byte stream.

## Decisions to confirm

- **D1** — the current "omit a `null` member" behaviour becomes the default option
  `Avm1IgnoreCondition.WhenWritingNull`, not a hard-coded rule.
- **D2** — attribute names align with STJ (nothing is released yet): `[Avm1PropertyName]` replaces the
  current `[Avm1Property]`; add `[Avm1PropertyOrder]`, `[Avm1Include]`, `[Avm1Constructor]`,
  `[Avm1Required]`, `[Avm1ExtensionData]`, `[Avm1Converter]`, `[Avm1Polymorphic]`, `[Avm1DerivedType]`.
  Keep `[Avm1Object]`, `[Avm1Ignore]`, `[Avm1Serializable]`.
- **D3** — `[Avm1Serializable]` binding path uses `.` as separator with a `Segments = [...]` escape hatch
  for keys that contain a `.`.

## 1. Principle: converter-centric, type-info is the spine

Every type is ultimately handled by an **`Avm1Converter<T>`** (values, objects, lists, dictionaries,
custom). The per-type metadata node, **`Avm1TypeInfo<T>`**, is the spine: it *carries* the converter
and (for objects) the property list. A **resolver** produces `Avm1TypeInfo`; **`Avm1SerializerOptions`**
orchestrates; **`Avm1Serializer`** is the public facade.

```
Avm1Serializer → Avm1SerializerOptions → IAvm1TypeInfoResolver → Avm1TypeInfo<T> → Avm1Converter<T>
                                          (reflection | generated context)
```

Both modes — reflection and source-gen — funnel through the **same** abstraction, with the **same**
converter-resolution precedence and the **same** `Kind` rule. That single funnel is what keeps the two
modes from diverging.

## 2. Core types

```csharp
namespace ShockwaveFlash.Avm1.Serialization;

public abstract class Avm1Converter
{
    public abstract Type Type { get; }
    public virtual bool CanConvert(Type typeToConvert) => typeToConvert == Type;
}

public abstract class Avm1Converter<T> : Avm1Converter
{
    public abstract T Read(Avm1Value value, Avm1SerializerOptions options);
    public abstract Avm1Value Write(T value, Avm1SerializerOptions options);
    public virtual bool HandleNull => false;                       // value-type converters override to true
    public virtual T ReadKey(string key, Avm1SerializerOptions o); // dictionary key (AVM1 keys are strings)
    public virtual string WriteKey(T value, Avm1SerializerOptions o);
}

public abstract class Avm1ConverterFactory : Avm1Converter        // List<T>, Dictionary<,>, enums, …
{
    public abstract Avm1Converter CreateConverter(Type typeToConvert, Avm1SerializerOptions options);
}

public enum Avm1TypeInfoKind { None, Object, List, Dictionary }   // derived from the resolved converter

public abstract class Avm1TypeInfo
{
    public Type Type { get; }
    public Avm1Converter Converter { get; }
    public Avm1TypeInfoKind Kind { get; }                         // read-only, set from Converter
    public IList<Avm1PropertyInfo> Properties { get; }            // meaningful only when Kind == Object
    public string[]? BindingPath { get; set; }                    // §6
    public void MakeReadOnly();
    public bool IsReadOnly { get; }
}

public sealed class Avm1TypeInfo<T> : Avm1TypeInfo
{
    public Func<T>? CreateObject { get; set; }
}

public sealed class Avm1PropertyInfo
{
    public string Name { get; set; }                             // the AVM1 key
    public Func<object, object?>? Get { get; set; }              // null => skip on write
    public Action<object, object?>? Set { get; set; }           // null => skip on read
    public Avm1Converter? CustomConverter { get; set; }
    public Avm1Converter EffectiveConverter { get; }            // resolved once, §4
    public Avm1NumberHandling? NumberHandling { get; set; }
    public Avm1IgnoreCondition? IgnoreCondition { get; set; }
    public Func<object, object?, bool>? ShouldSerialize { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
}

public interface IAvm1TypeInfoResolver
{
    Avm1TypeInfo? GetTypeInfo(Type type, Avm1SerializerOptions options);
}

public abstract class Avm1SerializerContext : IAvm1TypeInfoResolver { /* base for generated contexts */ }

public sealed class Avm1SerializerOptions
{
    public IList<Avm1Converter> Converters { get; }
    public IAvm1TypeInfoResolver? TypeInfoResolver { get; set; }       // kept in sync with the chain
    public IList<IAvm1TypeInfoResolver> TypeInfoResolverChain { get; }
    public Avm1NamingPolicy? PropertyNamingPolicy { get; set; }
    public Avm1NumberHandling NumberHandling { get; set; }
    public Avm1IgnoreCondition DefaultIgnoreCondition { get; set; }    // default: WhenWritingNull (D1)
    public bool IncludeFields { get; set; }
    public static Avm1SerializerOptions Default { get; }
    public void MakeReadOnly();
}

public static class Avm1Serializer
{
    public static Avm1Value Serialize<T>(T value, Avm1SerializerOptions? options = null);
    public static T Deserialize<T>(Avm1Value value, Avm1SerializerOptions? options = null);
    public static Avm1Value Serialize<T>(T value, Avm1TypeInfo<T> typeInfo);   // AOT path
    public static T Deserialize<T>(Avm1Value value, Avm1TypeInfo<T> typeInfo); // AOT path
    public static bool IsReflectionEnabledByDefault { get; }                  // link-time constant
}
```

The existing `Avm1Convert.ReadGlobal/WriteGlobal/ReadMap/WriteMap` become thin conveniences over
`Avm1Serializer`. The current per-type `ToAvm1Object()/FromAvm1Object()` stay as an optional zero-config
facade.

## 3. The central invariant

`Kind` is **read-only and derived from the resolved converter**. A type with a **custom converter is
opaque**: `Kind == None`, no configurable `Properties` — it is handled wholly by its converter (exactly
like STJ, where `Kind None` covers primitives, `object`, *and* every custom-converter type). This is the
one rule that keeps the model consistent; there are no special cases layered on top of it.

Built-in converters and their kinds:

| Converter | Handles | Kind |
|-----------|---------|------|
| `Avm1ObjectConverter<T>` | `[Avm1Object]` records/classes | `Object` |
| `Avm1EnumerableConverter<T,E>` | `T[]`, `List<T>`, `IEnumerable<T>` | `List` |
| `Avm1DictionaryConverter<V>` | `Dictionary<string,V>` and read-only/interface forms | `Dictionary` |
| scalar converters | `string`, `bool`, numerics, `enum` | `None` |
| `Avm1ValueConverter` | `Avm1Value`/`Avm1Object`/`Avm1Array` pass-through | `None` |
| any custom `Avm1Converter<T>` | the registered type | `None` |

## 4. Converter resolution (one fixed order)

Resolved once while building each `Avm1PropertyInfo`, stored in `EffectiveConverter`:

1. `[Avm1Converter]` on the **member** (property/field) — highest.
2. a converter in `Options.Converters` (first whose `CanConvert` returns true; factories included).
3. `[Avm1Converter]` on the **type**.
4. the built-in converter.

There is no API to reorder this — both modes share it.

## 5. Modes, resolver chain, AOT

- `Avm1TypeInfoResolver.Combine(resolverA, resolverB, …)` = **first non-null wins**. A resolver returning
  `null` means "not my type, fall through".
- `Options.TypeInfoResolver == null` ⇒ reflection (`DefaultAvm1TypeInfoResolver`).
- **Reflection mode** — `DefaultAvm1TypeInfoResolver` builds `Avm1TypeInfo` by reflection, with a list of
  `Modifiers` (`Action<Avm1TypeInfo>`) for contract customization. AOT-unsafe: the reflection entry
  points are annotated `[RequiresDynamicCode]`/`[RequiresUnreferencedCode]`.
- **Source-gen mode** — a generated `Avm1SerializerContext` *is* the resolver; reflection-free, AOT-safe.
  When the resolver returns `null` for a type, the serializer **fails hard and diagnosably** (no silent
  reflection fallback). Fallback is opt-in: `Avm1TypeInfoResolver.Combine(MyContext.Default, new DefaultAvm1TypeInfoResolver())`.
- AOT switch: a link-time-constant `Avm1Serializer.IsReflectionEnabledByDefault` plus the MSBuild property
  `Avm1SerializerIsReflectionEnabledByDefault` (auto-off under `PublishTrimmed`); branching on the constant
  does not root the reflection resolver under Native AOT.
- **Freeze discipline**: `Avm1TypeInfo`/`Avm1SerializerOptions` are mutable until first use, then
  `MakeReadOnly()` locks them; setters throw afterwards. All contract customization runs during resolution.

## 6. Source generator reframing

`[Avm1Serializable(typeof(T))]` on a `partial class MyContext : Avm1SerializerContext` makes the generator
emit an **`Avm1TypeInfo<T>`** (property `Get`/`Set` delegates, `CreateObject`/constructor binding, resolved
per-property converters) via an internal `Avm1MetadataServices.CreateObjectInfo<T>(options, Avm1ObjectInfoValues<T>)`
— the same flat-DTO → type-info bridge STJ uses. The context is the resolver; `MyContext.Default` is the
singleton.

- `[Avm1Object("EM")]` is now purely a **type-level binding path** read by the reflection resolver into
  `Avm1TypeInfo.BindingPath`; it no longer triggers generation. The per-type `ToAvm1Object/FromAvm1Object`
  facade and `IAvm1Serializable<T>`/`Avm1Convert` are **retired** — the context and `Avm1Serializer`
  (incl. `Read/WriteGlobal`) are the single entry points. Nested members no longer need a marker: any
  named non-scalar/non-collection type is treated as an object (reflection parity).
- **No fast-path.** STJ needs a separate serialize-only "fast-path" because it streams UTF-8. Over a value
  tree there is no async/buffering/positional dimension, so metadata mode is already allocation-light —
  we emit a single (metadata) path. The STJ rule "a custom converter forces the metadata path" is moot
  because there is only that path.
- **Advantage over STJ: private members.** STJ source-gen can only bind public/internal members (the
  context is a separate class). If the Avm1 generator emits each type's `Get`/`Set` accessors **inside the
  type's own `partial`** (and the context aggregates them), `[Avm1Include]` on a `private` member — and a
  `private` `[Avm1Constructor]` — work **without reflection**.

### Path binding on `[Avm1Serializable]`

The optional second argument binds a registered type to a **path in the globals container**, generalizing
the current single-segment `[Avm1Object("EM")]`:

```csharp
[Avm1Serializable(typeof(Emotes), "EM")]        // globals.EM
[Avm1Serializable(typeof(HouseList), "H.h")]    // globals.H.h
[Avm1Serializable(typeof(Foo), Segments = ["a.b", "c"])]   // escape for keys containing '.'
public partial class DofusLangContext : Avm1SerializerContext { }
```

- The path becomes `Avm1TypeInfo.BindingPath`; the context exposes navigating accessors:
  `ctx.Read<Emotes>(globals)` reads `globals["EM"]`; `ctx.Write(globals, emotes)` sets it, creating missing
  intermediate `Avm1Object`s on write and returning `default`/null (or throwing if required) on a missing
  segment on read.
- It **decouples a type from its location**: the same type can be registered at several paths, and a leaf
  type no longer needs to carry a global name.
- `[Avm1Object("EM")]` on the type supplies the path for reflection mode and as a fallback for context
  registration; an explicit `[Avm1Serializable]` path wins when both are present.
- Optional `TypeInfoPropertyName` names the generated accessor (`ctx.Emotes`) when a type is registered
  more than once.

## 7. Feature surface

**Essential**
- `Avm1Converter<T>` + `Avm1ConverterFactory` + `[Avm1Converter]` + the fixed precedence (§4).
- `Avm1NumberHandling` `[Flags]`: `Strict | AllowReadingFromString | WriteAsString | AllowNamedFloatingPointLiterals`.
  Genuinely needed for `Avm1Number(double)`: `NaN`/`Infinity` are **real AVM1 numeric values**, and AVM1
  coerces string→number.
- `[Avm1ExtensionData] Dictionary<string, Avm1Value>`: captures unknown members losslessly — a typed
  partial model plus an extension bag round-trips exactly, replacing whole-object pass-through.
- `Avm1NamingPolicy`, `Avm1IgnoreCondition` (generalizes the omit-null default),
  `[Avm1Include]`/`[Avm1Constructor]`/`[Avm1Required]`/`[Avm1PropertyOrder]`.

**High value**
- Polymorphism: `[Avm1Polymorphic]` + `[Avm1DerivedType(typeof(X), "discriminator")]` (or imperatively via a
  `PolymorphismOptions` modifier). Turns today's pass-through union fields (`monsters.g1`, `quests.r`, …)
  into real typed unions. The tree reader sidesteps STJ's "discriminator must come first" constraint
  because lookups are keyed.

**Skip (for now)**
- Reference handling / preserve-references — AVM1 data is acyclic.

## 8. What the tree model removes vs STJ

- No `Utf8JsonReader`/`Writer`, no async, no buffering, no positional "read too much / not enough"
  invariant. A converter receives **exactly one sub-tree** and returns one value.
- No serialize-only fast-path (§6).
- Dictionary keys are always strings ⇒ `ReadKey`/`WriteKey` are trivial.

The structural discipline that remains: a converter touches only its own sub-tree (never siblings/parents).

## 9. Phased plan

1. **Core** — converters, `Avm1TypeInfo`/`Avm1PropertyInfo`, resolver chain, `Avm1SerializerOptions`,
   `Avm1Serializer`, built-in converters; reframe the generator to emit `Avm1TypeInfo<T>` + the context;
   keep `[Avm1Object]` zero-config; reflection resolver (AOT-gated); path binding on `[Avm1Serializable]`.
2. **Customization** — `[Avm1Converter]` + factory + precedence, `[Avm1ExtensionData]`, `NumberHandling`,
   naming policy, ignore conditions, `[Avm1Include]`/`[Avm1Constructor]`/`[Avm1Required]`/`[Avm1PropertyOrder]`.
3. **Advanced** — polymorphism, contract `Modifiers`, AOT switches.
