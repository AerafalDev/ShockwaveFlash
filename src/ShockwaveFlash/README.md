# ShockwaveFlash

[![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.svg)](https://www.nuget.org/packages/ShockwaveFlash)
[![Downloads](https://img.shields.io/nuget/dt/ShockwaveFlash.svg)](https://www.nuget.org/packages/ShockwaveFlash)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE)

A fast, allocation-light reader and writer for the **SWF** (Shockwave Flash) binary format, for .NET 10.
Disassemble a `.swf` into a strongly-typed, mutable tag tree, edit it, then assemble it back — the
round-trip is **lossless** (byte-identical for canonically-encoded SWFs, validated on a real corpus).

```sh
dotnet add package ShockwaveFlash
```

```csharp
using ShockwaveFlash;
using ShockwaveFlash.Tags;

var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes("movie.swf"));

swf.Tags.RemoveAll(tag => tag.Metadata.Code is TagCode.Metadata);

File.WriteAllBytes("movie.trimmed.swf", swf.Assemble().ToArray());
```

## Documentation

**[SWF reading & writing →](https://aerafaldev.github.io/ShockwaveFlash/docs/swf)**

---

Part of the [ShockwaveFlash](https://github.com/AerafalDev/ShockwaveFlash) project ·
[MIT](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE) © Aerafal
