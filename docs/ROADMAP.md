# ROADMAP — toward a professional-grade SWF library

Prioritized audit and plan. Every item is anchored on `docs/swf-spec-19.pdf` (cited as
*§Chapter/section*) and ships with tests. Order follows the agreed phases; within a phase,
items are listed highest-impact first.

Legend: 🔴 blocker · 🟠 important · 🟡 nice-to-have.

---

## Current state (baseline)

- Reader-only, `ref struct SpanReader` over `ReadOnlySpan<byte>`; deferred payloads stored as
  `SpanSlice` offsets requiring the original buffer to be re-supplied.
- `FWS`/`CWS` decompress; `ZWS` (LZMA) throws. No writer. No tests. Generic exceptions.
- ~70 documented tags decoded; field-level fidelity unaudited; unknown tags rejected.

---

## Test foundation — stack decided 🔴

**Ordering decided: migrate first (Phase 2), then add tests.** There is no existing suite to keep
green, so the migration is validated by tests added immediately after it rather than before —
accepting that the migration itself runs without an automated net (mitigated by its
representation-only nature and a careful diff).

**Decided testing stack:**

- Base: `Microsoft.NET.Test.Sdk` + **xUnit** (`xunit`, `xunit.runner.visualstudio`) + `coverlet.collector`.
- Assertions: **Shouldly** (free/OSS — no licensing friction for an MIT NuGet package).
- **Verify.Xunit** — snapshot/approval tests for Phase 2 characterization and Phase 3 tag coverage.
- **CsCheck** — property-based round-trip invariants (Phase 4) + adversarial input generation (Phase 5).
- **BenchmarkDotNet** — perf gate for the Phase 2 hot-loop no-regression claim.
- **SharpFuzz** — fuzzing harness (Phase 5).

Steps:

- Add `tests/ShockwaveFlash.Tests/` (xUnit), wire it into `ShockwaveFlash.slnx`; `dotnet test` must
  discover and run it.
- Add **golden sample SWFs** under `tests/.../Assets/` (one `FWS`, one `CWS`; tiny hand-built and/or a
  small real file) as fixtures for round-trip and regression tests.
- Land characterization tests over reader output **right after** the Phase 2 migration, then keep them
  green through Phases 3–5.

---

## Phase 2 — Reader migration: `ref struct SpanReader` → `class MemoryReader` 🔴 (chantier a)

**Decision (overrides the original "keep hot loops on Span" brief):** drop `Span` as the reader's
storage/API currency entirely. The reader becomes a **reference type** over `ReadOnlyMemory<byte>` so
it is passed **by value with no `ref`** — its position is shared naturally. `Span` is used only where
strictly necessary (reading bytes out of `Memory`), never threaded or stored.

**Why:** `ref struct SpanReader` + `SpanSlice` cannot outlive a stack frame, forcing `ref` threading
through every `Decode` and forcing callers to re-supply the buffer for deferred payloads
(`DoActionTag.DecodeActions`, raw bitmap/ABC/sound). A `Memory`-backed class removes both problems and
unlocks storable/async-friendly readers.

- **`MemoryReader` (class)** over `ReadOnlyMemory<byte>` with an instance `Position`. Same primitive
  set as today; each read does `_memory.Span.Slice(...)` internally (the one place `Span` is necessary).
  No `ref` on the reader anywhere.
- **`BitReader` becomes a reference type too** (its bit-accumulator state currently spans calls via
  `ref`); it consumes the `MemoryReader` (passed by value) — no `ref`.
- **`SpanSlice` → `ReadOnlyMemory<byte>`**: deferred/raw payloads store a self-contained memory slice.
  `DoActionTag.DecodeActions` (and peers) no longer take a reader/buffer.
- All `Decode(ref SpanReader, …)` signatures across tags/types/actions become `Decode(MemoryReader, …)`.
- **Technical note:** a class cannot hold a `Span` field, so the old "grab `.Span` once, loop over it"
  optimization no longer applies at reader level; primitive reads go through `_memory.Span` per call.
  Accepted trade-off (simplicity/flexibility over micro-optimized hot loops). BenchmarkDotNet measures
  the actual cost; revisit only if a hot path proves problematic.
- This phase changes representation, not parse results.
- **Commit the migration on its own**, separate from coverage/feature work. Immediately follow with
  the xUnit project + characterization tests pinning the post-migration behaviour.

*Deliverable:* `MemoryReader`/`BitReader` reference types over `ReadOnlyMemory<byte>`, no `ref` in the
public decode surface, deferred payloads self-contained; characterization tests added right after.

---

## Phase 3 — Format coverage (reader) 🟠 (chantier c)

Audit each tag's fields against the spec and fill gaps, in the new memory/span style. For **each**
element: cite the spec section, implement parsing, add unit tests (fields, boundary values, one real
sample).

Priority targets (verify, then complete or confirm):

1. **Field-level audit** of tags currently keeping large raw `SpanSlice` payloads where the spec
   defines structure — e.g. confirm DefineBitsLossless(2) colour-table/zlib pixel data (*§8*),
   sound payload framing (*§11*), morph fill/line style arrays (*§9*) are modelled as intended.
2. **Fonts/Text** edge cases: DefineFont2/3 wide offsets, language codes, kerning records,
   `DefineFont4` (*§10*).
3. **Filters & blend modes** on PlaceObject3 (*§3*) — verify all 7 filter types and matrix layouts.
4. **DefineSceneAndFrameLabelData**, **DefineScalingGrid**, **SymbolClass** completeness (*§4*).
5. **Basic-type coverage**: ARGB record, color-transform-with-alpha, encoded U32 edge cases (*§1*).
6. AVM2/`DoABC`: keep raw for now; document as out-of-scope unless requested.

*Deliverable:* per-tag spec citation + tests; coverage table in CLAUDE.md updated from ⚠️ to ✅.

---

## Phase 4 — Writer (new) 🔴 (chantier b)

Symmetric to the reader: anything the reader reads, the writer writes, per spec.

- Introduce a `SpanWriter`/`BufferWriter` (grow-able `IBufferWriter<byte>` backed) + `BitWriter`
  mirroring `SpanReader`/`BitReader`, with the same primitive set (LE ints, fixed-point, encoded U32,
  bit fields, strings).
- Each tag/type gains an `Encode` symmetric to `Decode`; `RECORDHEADER` short/long-form emission with
  correct length back-patching; movie header + file-length fixup.
- **Compression on write:** emit `FWS`/`CWS` (ZLib) and `ZWS` (LZMA) — pairs with the LZMA gap below.
- **Primary test: round-trip.** Read a real SWF → write → assert equality. Byte-exact where the format
  is canonical; otherwise semantic equality after re-reading. Also test writing hand-built structures.

*Deliverable:* `ShockwaveFlashFile.Assemble`/`Write`, round-trip tests on the golden samples, builder tests.

### LZMA support 🟠 (closes the `ZWS` gap, both directions)

- `.NET 10` has no BCL LZMA. Add a dependency (e.g. `SharpCompress`, or a vetted LZMA SDK) and implement
  `Lzma.Decompress` **and** compress, matching the SWF LZMA framing (*§2 file compression*; note SWF's
  LZMA header differs from raw `.lzma`). Round-trip a `ZWS` sample.

---

## Phase 5 — Robustness + fuzzing 🟠 (chantiers d, e)

Make the parser uncrashable on malformed input: truncation, inconsistent tag lengths, recursion/cycles
(nested sprites, clip actions), corrupt compression. Always a clean, **typed** exception — never an
unguarded crash.

- Introduce a typed exception hierarchy (e.g. `SwfException` → `SwfFormatException`,
  `SwfTruncatedException`, `SwfUnsupportedException`, `SwfCompressionException`). Replace the bare
  `throw new Exception(...)` (tag-length mismatch) and audit `NotSupportedException`/`ArgumentOutOfRange`
  sites so callers can catch meaningfully.
- **Preserve unknown/obsolete tags** as an opaque `UnknownTag` (raw bytes) instead of throwing, so
  real-world files don't fail wholesale. (App. B reverse index for the known universe.)
- Bound recursion depth (nested `DefineSprite`/clip actions) and reject pathological lengths defensively;
  guard `ReadNullTerminatedString` and decompressed-size limits.
- **Fuzzing with SharpFuzz:** harness over `Disassemble`, ideally a second harness for
  read→write→read round-trip stability. Run a short campaign, fix every crash, and add each crashing
  input to `tests/.../Assets/Fuzz/` as a regression test.

*Deliverable:* typed exceptions, opaque unknown-tag support, SharpFuzz harness(es), regression corpus.

---

## Phase 6 — Packaging, CI, docs 🟠 (chantier f)

- **NuGet:** package metadata on `ShockwaveFlash.csproj` (PackageId, authors, license expression
  `MIT`, repository URL, README, symbols/`snupkg`, deterministic build), **SemVer** versioning
  (start `0.x` pre-1.0). `dotnet pack` produces a clean package locally.
- **README** (user-facing): install, quick-start **reader AND writer** examples, supported features
  table, link to spec coverage.
- **CI:** extend `.github/workflows/ci.yml` to `build` + `test` + `dotnet format --verify-no-changes`
  on every push/PR. Keep CodeQL. (A separate publish workflow may be prepared but left disabled.)
- **No publish, no `git push`** — prepare everything; final manual steps (API key/secret, tag, publish)
  documented for the maintainer.

---

## Cross-cutting (track throughout)

- Keep `CLAUDE.md` coverage table honest as ⚠️ items become ✅.
- Frequent atomic commits; tests with every code change; spec citations over assumptions.
- Summarize each phase before starting the next.
