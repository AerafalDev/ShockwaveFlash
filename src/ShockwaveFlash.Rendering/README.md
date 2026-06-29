# ShockwaveFlash.Rendering

[![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.Rendering.svg)](https://www.nuget.org/packages/ShockwaveFlash.Rendering)
[![Downloads](https://img.shields.io/nuget/dt/ShockwaveFlash.Rendering.svg)](https://www.nuget.org/packages/ShockwaveFlash.Rendering)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE)

Render SWF characters parsed by [**ShockwaveFlash**](https://www.nuget.org/packages/ShockwaveFlash) —
shapes, sprites, timelines, morph shapes, text and images — to **SVG** and to raster images
(**PNG / JPEG / WebP / animated GIF / PDF**) through a native, cross-platform **Skia** backend. No
external tools — no Inkscape, rsvg or ImageMagick.

```sh
dotnet add package ShockwaveFlash.Rendering
```

> On headless Linux, also add `SkiaSharp.NativeAssets.Linux.NoDependencies`.

```csharp
using ShockwaveFlash;
using ShockwaveFlash.Rendering;
using ShockwaveFlash.Rendering.Drawing.Skia;

var renderer = new SwfRenderer(ShockwaveFlashFile.Disassemble(File.ReadAllBytes("movie.swf")));

File.WriteAllBytes("movie.png", SkiaDrawer.RenderToPng(renderer.Movie(), scale: 2f));
```

## Documentation

**[Rendering →](https://aerafaldev.github.io/ShockwaveFlash/docs/rendering)**

---

Part of the [ShockwaveFlash](https://github.com/AerafalDev/ShockwaveFlash) project ·
[MIT](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE) © Aerafal
