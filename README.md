# ShockwaveFlash

[![Build](https://github.com/AerafalDev/ShockwaveFlash/actions/workflows/ci.yml/badge.svg)](https://github.com/AerafalDev/ShockwaveFlash/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Read, edit and write **SWF** (Shockwave Flash) files — and their AVM1 ActionScript — entirely in code, on .NET 10. Fast, allocation-light, and round-trips byte-for-byte.

## Packages

| Package | Version | Description |
| --- | --- | --- |
| [**ShockwaveFlash**](src/ShockwaveFlash/README.md) | [![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.svg)](https://www.nuget.org/packages/ShockwaveFlash) | The SWF container: disassemble a `.swf` into a strongly-typed tag tree and assemble it back, byte-identical. |
| [**ShockwaveFlash.Avm1**](src/ShockwaveFlash.Avm1/README.md) | [![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.Avm1.svg)](https://www.nuget.org/packages/ShockwaveFlash.Avm1) | AVM1 `DoAction` bytecode: decode actions, evaluate data scripts to a typed value tree, edit, and write back. |

## Highlights

- **Byte-identical round-trip** — parse a real-world `.swf` and re-assemble the exact same bytes.
- **Broad tag coverage** — shapes, fonts, text, sounds, bitmaps, sprites, buttons, morph shapes, video, ABC, filters; unknown tags are kept verbatim.
- **AVM1 data scripts** — evaluate localization/config bytecode into a typed value tree, edit it, and write it back.
- **Typed & allocation-light** — immutable records over `ReadOnlyMemory<byte>`, exact fixed-point types, typed exceptions; no `unsafe` in your path.

## Getting started

```sh
dotnet add package ShockwaveFlash        # read / write SWF
dotnet add package ShockwaveFlash.Avm1   # AVM1 bytecode & data scripts
```

Usage and examples live in each package's README, linked in the table above.

## Contributing

Issues and pull requests are welcome — see [CONTRIBUTING](CONTRIBUTING.md).

## License

[MIT](LICENSE) © Aerafal
