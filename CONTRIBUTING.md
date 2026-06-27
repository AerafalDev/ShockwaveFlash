# Contributing to ShockwaveFlash

Thanks for taking the time to contribute! This project is a reader, writer and
renderer for the **SWF** (Shockwave Flash) binary format on .NET. Issues, bug
reports and pull requests are all welcome.

By participating you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md).

## Getting started

You need the **.NET 10 SDK**. The exact band is pinned in [`global.json`](global.json),
so the right SDK is selected automatically.

```sh
git clone https://github.com/AerafalDev/ShockwaveFlash.git
cd ShockwaveFlash

dotnet build -c Release        # build every project
dotnet test  -c Release        # run the test suite
```

To dump a SWF while developing:

```sh
dotnet run --project src/ShockwaveFlash.Playground -- path/to/movie.swf
```

## The specifications come first

The `docs/` folder holds the format specs, and **they are the authority** — they
override any assumption in code or in an issue:

- `swf-spec-19.pdf` — the SWF container, every tag, shapes, fonts/text, bitmaps,
  sounds, buttons, sprites, video, morph shapes and metadata.
- `ecma-262-3.pdf`, `learning-as2.pdf`, `learning-as3.pdf` — ActionScript semantics
  behind AVM1/AVM2.

When you add or change how a tag or structure is parsed/written, cite the exact
spec chapter/section in the pull request.

## Coding conventions

Style is enforced by [`.editorconfig`](.editorconfig); please don't fight it. The
points that matter most here:

- **C# style** — file-scoped namespaces, 4-space indentation, Allman braces, `var`
  when the type is apparent, `_camelCase` private fields, `s_` static fields.
- **No comments** — the code carries no inline or XML-doc comments; put the *why*
  in the commit message and the PR description instead.
- **No primary constructors** — declare constructors explicitly.
- **Member order** — fields, then properties, then constructors, then methods.
- **Data shapes** — immutable `readonly struct` for value types, mutable classes
  for the editable tag tree, `IReadOnlyList<T>` for exposed collections.
- **Reader/writer hygiene** — all reads go through the IO primitives; never index a
  raw span ad hoc, and route bit-packed fields through the bit reader.
- **Warnings are errors** — the build runs with `TreatWarningsAsErrors`, so a green
  build means zero warnings.

Files are **UTF-8 (no BOM) with CRLF** line endings, normalized by `.gitattributes`.

## Tests

New behaviour ships with a test. The suite lives in `src/ShockwaveFlash.Tests`
(xUnit + Shouldly, with CsCheck for property-based round-trip checks). The guiding
invariant is **lossless round-trip**: disassembling and re-assembling a SWF must
preserve the model, and a second pass must be byte-stable. Run `dotnet test -c Release`
before opening a pull request.

## Pull requests

- Branch off `main`; keep each change small and self-contained.
- Write clear, present-tense commit messages — one logical change per commit.
- Make sure `dotnet build -c Release` and `dotnet test -c Release` are green.
- Describe *what* changed and *why*, and link the relevant spec section or issue.

## Reporting bugs

Open an issue with the SWF version involved, what you expected, what happened, and
a minimal sample if you can share one. For security-sensitive reports, follow the
[Security Policy](SECURITY.md) instead of opening a public issue.
