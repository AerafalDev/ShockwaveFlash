# ShockwaveFlash

[![Build](https://github.com/AerafalDev/ShockwaveFlash/actions/workflows/ci.yml/badge.svg)](https://github.com/AerafalDev/ShockwaveFlash/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Read, edit and write **SWF** (Shockwave Flash) files — their AVM1 ActionScript, and render them to images or SVG — entirely in code, on .NET 10. Fast, allocation-light, and round-trips losslessly.

## Packages

| Package | Version | Description |
| --- | --- | --- |
| [**ShockwaveFlash**](src/ShockwaveFlash/README.md) | [![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.svg)](https://www.nuget.org/packages/ShockwaveFlash) | The SWF container: disassemble a `.swf` into a strongly-typed, mutable tag tree and assemble it back losslessly. |
| [**ShockwaveFlash.Avm1**](src/ShockwaveFlash.Avm1/README.md) | [![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.Avm1.svg)](https://www.nuget.org/packages/ShockwaveFlash.Avm1) | AVM1 `DoAction` bytecode: decode actions, evaluate data scripts to a typed value tree, edit, and write back. |
| [**ShockwaveFlash.Rendering**](src/ShockwaveFlash.Rendering/README.md) | [![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.Rendering.svg)](https://www.nuget.org/packages/ShockwaveFlash.Rendering) | Render shapes, sprites, timelines and text to **PNG/JPEG/WebP/GIF** (Skia) and **SVG** — no external tools. |

## Highlights

- **Lossless round-trip** — parse a real-world `.swf` and re-assemble it without losing data; byte-identical for canonically-encoded files.
- **Broad tag coverage** — shapes, fonts, text, sounds, bitmaps, sprites, buttons, morph shapes, video, ABC, filters; unknown tags are kept verbatim.
- **AVM1 data scripts** — evaluate localization/config bytecode into a typed value tree, edit it, and write it back.
- **Typed & allocation-light** — a mutable, strongly-typed tag tree over `ReadOnlyMemory<byte>`, exact fixed-point value types, and typed exceptions.

## Getting started

```sh
dotnet add package ShockwaveFlash             # read / write SWF
dotnet add package ShockwaveFlash.Avm1        # AVM1 bytecode & data scripts
dotnet add package ShockwaveFlash.Rendering   # render to PNG/JPEG/WebP/GIF/SVG
```

Usage and examples live in each package's README, linked in the table above.

## Contributing

Issues and pull requests are welcome — see [CONTRIBUTING](CONTRIBUTING.md).

## License

[MIT](LICENSE) © Aerafal
