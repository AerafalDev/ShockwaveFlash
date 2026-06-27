# ShockwaveFlash

[![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.svg)](https://www.nuget.org/packages/ShockwaveFlash)
[![Downloads](https://img.shields.io/nuget/dt/ShockwaveFlash.svg)](https://www.nuget.org/packages/ShockwaveFlash)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE)

A fast, allocation-light reader and writer for the **SWF** (Shockwave Flash) binary format, for .NET 10.

Disassemble a `.swf` into a strongly-typed tag tree, inspect or edit it in code, then assemble it back — the round-trip is **lossless**: byte-identical for canonically-encoded SWFs and byte-stable on re-encode, validated on a real corpus of production files.

## Install

```sh
dotnet add package ShockwaveFlash
```

## Read

```csharp
using ShockwaveFlash;

var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes("movie.swf"));

Console.WriteLine($"SWF v{swf.Header.Version} ({swf.Header.Compression})");
Console.WriteLine($"Frame size : {swf.Header.FrameSize}");
Console.WriteLine($"Frame rate : {swf.Header.FrameRate.ToSingle()} fps");
Console.WriteLine($"Tags       : {swf.Tags.Count}");

foreach (var tag in swf.Tags)
    Console.WriteLine($"  {tag.Metadata.Code} ({tag.Metadata.Length} bytes)");
```

## Edit & write

The movie and its tags are mutable, so edit them in place — `Tags` is a `List<Tag>` and every tag exposes settable properties:

```csharp
using ShockwaveFlash;
using ShockwaveFlash.Tags;

var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes("movie.swf"));

swf.Tags.RemoveAll(tag => tag.Metadata.Code is TagCode.Metadata);

File.WriteAllBytes("movie.trimmed.swf", swf.Assemble().ToArray());
```

## Round-trip

```csharp
var original = File.ReadAllBytes("movie.swf");
var rebuilt = ShockwaveFlashFile.Disassemble(original).Assemble();

// true for canonically-encoded SWFs; always byte-stable on a second pass.
bool identical = rebuilt.Span.SequenceEqual(original);
```

## Error handling

Malformed input throws a typed `SwfException` rather than corrupting state:

```csharp
using ShockwaveFlash.Exceptions;

try
{
    var swf = ShockwaveFlashFile.Disassemble(bytes);
}
catch (SwfTruncatedException) { /* buffer ended mid-record */ }
catch (SwfFormatException)    { /* not a valid SWF */ }
```

## Notes

- **Coverage** — shapes, fonts, text, sounds, bitmaps, sprites, buttons, morph shapes, video, ABC, filters and control tags. Unknown tags are preserved verbatim, so nothing is lost on round-trip.
- **Exact wire model** — fixed-point types (`Fixed16` 16.16, `Fixed8` 8.8); no lossy `float` conversions.
- **Compression** — `FWS` (uncompressed), `CWS` (zlib) and `ZWS` (LZMA), all read and write.
- **No `unsafe`, no `Span` plumbing** — the public surface works over `ReadOnlyMemory<byte>`.

---

Part of the [ShockwaveFlash](https://github.com/AerafalDev/ShockwaveFlash) project · [MIT](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE) © Aerafal
