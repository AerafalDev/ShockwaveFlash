# CLAUDE.md

Guidance for working in this repository. Read this first; the `docs/*.pdf` specs are
the authority on the SWF format and override any assumption made here or in code.

## Objective

`ShockwaveFlash` is a high-performance, allocation-conscious **reader (and, in
progress, writer)** for the Adobe **SWF** (Shockwave Flash) binary file format,
targeting modern .NET. The goal is a professional-grade, NuGet-published library that
can parse and (re)emit SWF files faithfully, robustly, and fast.

## Specifications (`docs/`)

| File | Authority on |
|------|--------------|
| `swf-spec-19.pdf` | **THE** authority for the SWF container, all tags, basic data types, shapes, fonts/text, bitmaps, sounds, buttons, sprites, video, morph shapes, metadata. Cite chapter/section for every format detail. |
| `ecma-262-3.pdf` | ECMAScript 3 semantics underlying AVM1 ActionScript (operators, type coercion). Reference for action behaviour, not byte layout. |
| `learning-as2.pdf` | ActionScript 2 language guide. Background for AVM1 actions. |
| `learning-as3.pdf` | ActionScript 3 language guide. Background for AVM2 / `DoABC`. |
| `flash-player-admin-32.pdf` | Flash Player runtime/admin behaviour. Background only. |

SWF spec chapter map (v19): 1 Basic Data Types · 2 SWF Structure · 3 Display List ·
4 Control Tags · 5 Actions · 6 Shapes · 7 Gradients · 8 Bitmaps · 9 Shape Morphing ·
10 Fonts & Text · 11 Sounds · 12 Buttons · 13 Sprites · 14 Video · 15 Metadata ·
App. A worked example · App. B reverse tag index.

## Architecture

```text
src/
  ShockwaveFlash/                 ← the library (net10.0)
    ShockwaveFlashFile.cs         ← entry point: Disassemble(ref SpanReader)
    ShockwaveFlashHeader.cs       ← signature/version/frameSize/rate/count
    ShockwaveFlashCompression.cs  ← FWS/CWS/ZWS enum + Decompress dispatch
    IO/
      Binary/SpanReader.cs        ← ref struct over ReadOnlySpan<byte>; all primitive reads
      Bits/BitReader.cs           ← MSB-first bit reader (UB/SB/FB/bit), drives SpanReader
      Buffers/SpanSlice.cs        ← (offset,length) into the buffer — deferred/raw payloads
      Compression/{ZLib,Lzma}.cs  ← decompress only
      Extensions/                 ← BinaryPrimitives helpers (UInt24, …)
    Tags/                         ← one record per tag, grouped by domain; Tag.cs dispatches code→record
    Types/                        ← shared records (Rectangle, Matrix, Color, Shape*, Filter*, Font*, …)
    Actions/Avm1/                 ← AVM1 bytecode: one record per action, grouped by SWF version
  ShockwaveFlash.Playground/      ← console smoke-test app (dumps tags / disassembles actions)
```

### Reading model

- `ShockwaveFlashFile.Disassemble(ref SpanReader)` reads the 8-byte header, decompresses
  the body (if `CWS`/`ZWS`), re-wraps the decompressed bytes in a new `SpanReader`, decodes
  the movie header, then `Tag.DecodeCollection`.
- `Tag.DecodeCollection` loops the tag stream: `RECORDHEADER` (`code<<6 | length`, with the
  long form when `length == 63`), slices the tag body into a **sub-reader**, dispatches on
  `TagCode`, and verifies the body was fully consumed.
- Each tag exposes a static `Decode(ref SpanReader, TagMetadata, …)` and is an immutable
  `record`. `BitReader` is threaded by `ref` alongside `SpanReader` for bit-packed structures
  (shapes, matrices, rectangles).
- **`SpanSlice` indirection:** raw/opaque or lazily-decoded payloads (action bytecode, JPEG
  data, ABC, sound/video frames, binary data) are stored as `SpanSlice` (offset+length), **not**
  copied. Decoding them later (e.g. `DoActionTag.DecodeActions`) requires re-supplying the
  original buffer/reader. This is the main motivation for the planned migration (see ROADMAP):
  `ref struct SpanReader` + `SpanSlice` → a **`class MemoryReader` over `ReadOnlyMemory<byte>`**,
  passed by value with **no `ref`**, deferred payloads stored as self-contained `ReadOnlyMemory<byte>`.
  `Span` is kept only internally where reading bytes requires it.

## Commands

```bash
dotnet build                                  # build all (net10.0)
dotnet build -c Release
dotnet test                                   # run tests  (NO test project exists yet — see ROADMAP)
dotnet format                                 # apply formatting/style from .editorconfig
dotnet format --verify-no-changes             # CI gate (not wired up yet)
dotnet run --project src/ShockwaveFlash.Playground -- <file.swf>   # dump a SWF
```

## Conventions

- **TFM:** `net10.0`, `ImplicitUsings` + `Nullable` enabled, `AllowUnsafeBlocks`.
- **Style:** enforced by `.editorconfig` — 4-space indent, file-scoped namespaces, Allman braces,
  `var` when the type is apparent, `_camelCase` private fields, `s_` static fields, PascalCase
  constants, `System` usings first, **license header on every `.cs` file**:
  ```csharp
  // Copyright (c) Aerafal 2026.
  // Licensed under the MIT license.
  // See the LICENSE file in the project root for more information.
  ```
- **Data shapes:** prefer immutable `record` / `readonly struct`; `IReadOnlyList<T>` for collections.
- **Reader hygiene:** all reads go through `SpanReader`; never index the raw span ad hoc. Bit-packed
  fields go through `BitReader`. New tag layouts cite the exact spec section in a comment.
- **One change = one verifiable result**; tests ship with the code; commit small and atomically.

## Format coverage — managed vs missing

### Container / compression

- ✅ `FWS` uncompressed, ✅ `CWS` ZLib (decompress). Header, `RECORDHEADER` long/short form, sub-reader length check.
- ❌ `ZWS` **LZMA decompress** — `Lzma.Decompress` throws `NotSupportedException` (no BCL LZMA). **SPEC GAP.**
- ❌ **Writing/compression of any kind** — there is no writer at all yet.

### Tags (reader) — `TagCode` covers the documented set

- ✅ Display list: PlaceObject 1/2/3, RemoveObject 1/2, ShowFrame.
- ✅ Control: SetBackgroundColor, FrameLabel, Protect, End, Export/ImportAssets(2), EnableDebugger(2),
  ScriptLimits, SetTabIndex, FileAttributes, SymbolClass, Metadata, DefineScalingGrid,
  DefineSceneAndFrameLabelData, DebugId, ProductInfo, NameCharacter.
- ✅ Shapes 1–4, Gradients, Morph 1/2. ✅ Bitmaps: DefineBits, JPEGTables, JPEG2/3/4, Lossless 1/2.
- ✅ Fonts: DefineFont 1/2/3/4, FontInfo 1/2, AlignZones, FontName; Text 1/2, EditText, CSMTextSettings.
- ✅ Sounds: DefineSound, StartSound(2), SoundStreamHead(2), SoundStreamBlock, ButtonSound.
- ✅ Buttons 1/2, ButtonCxform. ✅ Sprites. ✅ Video: DefineVideoStream, VideoFrame.
- ✅ Actions: DoAction, DoInitAction, DoABC/DoABC2 (ABC payload kept raw). Metadata: DefineBinaryData, EnableTelemetry.
- ⚠️ **Field-level completeness is unaudited** — several tags keep payloads as raw `SpanSlice`
  (JPEG/ABC/sound/video frames, binary data) rather than fully-typed structures; this is fine
  for round-trip but means semantic coverage must be verified tag-by-tag against the spec (Phase 3).
- ❌ Unknown/obsolete tag codes are **rejected** (`NotSupportedException`) instead of preserved as an
  opaque tag — brittle against real-world files. (See ROADMAP robustness.)

### Actions (AVM1)

- ✅ SWF 1–7 opcodes implemented as individual records; `ActionUnknown` fallback for unknown opcodes.
- ⚠️ AVM2 (`DoABC`) bytecode is stored raw, not parsed.

### Robustness / errors

- ⚠️ Errors are generic: `NotSupportedException`, `ArgumentOutOfRangeException`, `InvalidOperationException`,
  and a bare `Exception` for tag-length mismatch. **No typed `Swf…Exception` hierarchy.** Malformed
  input handling is incidental, not designed. (Phase 5.)

### Tooling

- ✅ CI builds + runs `dotnet test`; ✅ CodeQL; ✅ Dependabot. ❌ no `dotnet format` gate.
- ❌ **No test project / no tests** of any kind. ❌ no NuGet packaging. ❌ empty README.

See `docs/ROADMAP.md` for the prioritized plan to close these gaps.
