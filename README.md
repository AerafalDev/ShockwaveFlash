# ShockwaveFlash

[![Build](https://github.com/AerafalDev/ShockwaveFlash/actions/workflows/ci.yml/badge.svg)](https://github.com/AerafalDev/ShockwaveFlash/actions/workflows/ci.yml)
[![Docs](https://github.com/AerafalDev/ShockwaveFlash/actions/workflows/docs.yml/badge.svg)](https://aerafaldev.github.io/ShockwaveFlash/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Read, edit and write **SWF** (Shockwave Flash) files — their AVM1 ActionScript, and render them to
images or SVG — entirely in code, on **.NET 10**. Fast, allocation-light, and round-trips losslessly.

## Documentation

**[aerafaldev.github.io/ShockwaveFlash](https://aerafaldev.github.io/ShockwaveFlash/)** — guides for SWF
reading/writing, AVM1 bytecode, the `System.Text.Json`-style serializer, and rendering.

## Packages

| Package | Version | Description |
| --- | --- | --- |
| [**ShockwaveFlash**](src/ShockwaveFlash/README.md) | [![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.svg)](https://www.nuget.org/packages/ShockwaveFlash) | Disassemble a `.swf` into a strongly-typed, mutable tag tree and assemble it back losslessly. |
| [**ShockwaveFlash.Avm1**](src/ShockwaveFlash.Avm1/README.md) | [![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.Avm1.svg)](https://www.nuget.org/packages/ShockwaveFlash.Avm1) | Decode `DoAction` bytecode, evaluate data scripts to a typed value tree, edit, and map to your own records. |
| [**ShockwaveFlash.Rendering**](src/ShockwaveFlash.Rendering/README.md) | [![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.Rendering.svg)](https://www.nuget.org/packages/ShockwaveFlash.Rendering) | Render shapes, sprites, timelines and text to **PNG/JPEG/WebP/GIF/PDF** (Skia) and **SVG**. |

```sh
dotnet add package ShockwaveFlash             # read / write SWF
dotnet add package ShockwaveFlash.Avm1        # AVM1 bytecode & data scripts
dotnet add package ShockwaveFlash.Rendering   # render to PNG/JPEG/WebP/GIF/PDF/SVG
```

## Contributing

Issues and pull requests are welcome — see [CONTRIBUTING](CONTRIBUTING.md).

## License

[MIT](LICENSE) © Aerafal
