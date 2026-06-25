# ShockwaveFlash

A fast, allocation-light reader and writer for the **SWF** (Shockwave Flash) binary format, written for .NET 10.

Disassemble a `.swf` file into a strongly-typed tag tree, inspect or edit it in code, then assemble it back. Re-assembly is **byte-identical** to the input it was parsed from — validated by round-tripping a real corpus of production files.

## Features

- **Reader and writer** for the SWF container and its tag stream — every supported tag round-trips.
- **Byte-identical** assembly: parse then assemble reproduces the original bytes exactly.
- **Broad tag coverage** (shapes, fonts, text, sounds, bitmaps, sprites, buttons, morph shapes, video, ABC, filters, control tags). Unknown tags are preserved verbatim, so nothing is lost on round-trip.
- **Exact fixed-point types** (`Fixed16` 16.16, `Fixed8` 8.8) — no lossy `float` conversions in the wire model.
- **Compression**: reads `FWS` (uncompressed), `CWS` (zlib) and `ZWS` (LZMA); writes uncompressed and zlib.
- **Typed exceptions** (`SwfFormatException`, `SwfTruncatedException`, `SwfUnsupportedException`, `SwfCompressionException`) for precise error handling on malformed input.
- **No `unsafe` in your code path, no `Span` plumbing** — the public surface works over `ReadOnlyMemory<byte>`.

## Install

```sh
dotnet add package ShockwaveFlash
```

## Quick start

### Read

```csharp
using ShockwaveFlash;

var bytes = File.ReadAllBytes("movie.swf");
var swf = ShockwaveFlashFile.Disassemble(bytes);

Console.WriteLine($"SWF v{swf.Header.Version} ({swf.Header.Compression})");
Console.WriteLine($"Frame size : {swf.Header.FrameSize}");
Console.WriteLine($"Frame rate : {swf.Header.FrameRate.ToSingle()} fps");
Console.WriteLine($"Frames     : {swf.Header.FrameCount}");
Console.WriteLine($"Tags       : {swf.Tags.Count}");

foreach (var tag in swf.Tags)
    Console.WriteLine($"  {tag.Metadata.Code} ({tag.Metadata.Length} bytes)");
```

### Write

`ShockwaveFlashFile` is an immutable record, so edits are expressed with `with`:

```csharp
using ShockwaveFlash;
using ShockwaveFlash.Tags;

var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes("movie.swf"));

var trimmed = swf with
{
    Tags = swf.Tags.Where(tag => tag.Metadata.Code is not TagCode.Metadata).ToList()
};

ReadOnlyMemory<byte> output = trimmed.Assemble();
File.WriteAllBytes("movie.trimmed.swf", output.ToArray());
```

### Round-trip

```csharp
var original = File.ReadAllBytes("movie.swf");
var rebuilt = ShockwaveFlashFile.Disassemble(original).Assemble();

// Reproduces the original bytes exactly.
bool identical = rebuilt.Span.SequenceEqual(original);
```

## Error handling

Malformed input throws a `SwfException` (or one of its derived types) rather than corrupting state:

```csharp
using ShockwaveFlash.Exceptions;

try
{
    var swf = ShockwaveFlashFile.Disassemble(bytes);
}
catch (SwfTruncatedException)
{
    // the buffer ended before a record was complete
}
catch (SwfFormatException)
{
    // the bytes are not a valid SWF
}
```

## License

[MIT](LICENSE) © Aerafal
