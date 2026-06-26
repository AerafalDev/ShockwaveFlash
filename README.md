# ShockwaveFlash

[![Build](https://github.com/AerafalDev/ShockwaveFlash/actions/workflows/ci.yml/badge.svg)](https://github.com/AerafalDev/ShockwaveFlash/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A fast, allocation-light toolkit for the **SWF** (Shockwave Flash) binary format on .NET 10 — read, edit and write SWF files, and their AVM1 ActionScript, entirely in code.

## Packages

| Package | Description |
|---|---|
| [**ShockwaveFlash**](src/ShockwaveFlash/README.md) [![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.svg)](https://www.nuget.org/packages/ShockwaveFlash) | The SWF container: disassemble a `.swf` into a strongly-typed tag tree and assemble it back, byte-identical. |
| [**ShockwaveFlash.Avm1**](src/ShockwaveFlash.Avm1/README.md) [![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.Avm1.svg)](https://www.nuget.org/packages/ShockwaveFlash.Avm1) | AVM1 `DoAction` bytecode: decode actions, evaluate data scripts to a typed value tree, edit, and write back. |

## At a glance

```csharp
using ShockwaveFlash;

var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes("movie.swf"));

Console.WriteLine($"SWF v{swf.Header.Version} — {swf.Tags.Count} tags");

// Re-assembly is byte-identical to the input it was parsed from.
ReadOnlyMemory<byte> rebuilt = swf.Assemble();
```

Head to each package's README for full usage:

- **[src/ShockwaveFlash](src/ShockwaveFlash/README.md)** — reading and writing SWF.
- **[src/ShockwaveFlash.Avm1](src/ShockwaveFlash.Avm1/README.md)** — AVM1 bytecode and data scripts.

## License

[MIT](LICENSE) © Aerafal
