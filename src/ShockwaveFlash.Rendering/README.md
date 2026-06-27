# ShockwaveFlash.Rendering

[![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.Rendering.svg)](https://www.nuget.org/packages/ShockwaveFlash.Rendering)
[![Downloads](https://img.shields.io/nuget/dt/ShockwaveFlash.Rendering.svg)](https://www.nuget.org/packages/ShockwaveFlash.Rendering)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE)

Render SWF characters — shapes, sprites, timelines, morph shapes, text and images — parsed by [**ShockwaveFlash**](https://www.nuget.org/packages/ShockwaveFlash) to **SVG** and to raster images (**PNG / JPEG / WebP / animated GIF / PDF**) through a native, cross-platform **Skia** backend. No external tools — no Inkscape, rsvg or ImageMagick.

## Install

```sh
dotnet add package ShockwaveFlash.Rendering
```

> On headless Linux, also add `SkiaSharp.NativeAssets.Linux.NoDependencies`.

## Render to an image

```csharp
using ShockwaveFlash;
using ShockwaveFlash.Rendering;
using ShockwaveFlash.Rendering.Drawing.Skia;

var swf = ShockwaveFlashFile.Disassemble(File.ReadAllBytes("movie.swf"));
var renderer = new SwfRenderer(swf);

// the whole movie, scaled 2x...
byte[] png = SkiaDrawer.RenderToPng(renderer.Movie(), scale: 2f);
File.WriteAllBytes("movie.png", png);

// ...or a single character by id
byte[] jpeg = SkiaDrawer.RenderToJpeg(renderer.Character(42), quality: 90);
```

`SkiaDrawer` also exposes `RenderToWebp`, `RenderToPdf` and `RenderToAnimatedGif` (which walks every frame of the timeline).

## Render to SVG

```csharp
using ShockwaveFlash.Rendering.Drawing.Svg;

string svg = SvgDrawer.RenderToSvg(renderer.Movie());
File.WriteAllText("movie.svg", svg);
```

## Design

A clean layering, [inspired](https://github.com/Arakne/ArakneSwf) by Arakne-Swf but with rasterization pulled in-process via Skia:

```
ShockwaveFlash                parser / writer
   └─ ShockwaveFlash.Rendering
        ├─ Model/       immutable drawable data (shapes, images, morphs, …)
        ├─ Processing/  tags → model (ShapeProcessor, TimelineProcessor, …)
        ├─ Scene/       display list (frames, blend modes)
        └─ Drawing/     IDrawer visitor + Skia & SVG backends
```

- **Skia is the native rasterizer** — exact gradients, blend modes, filters and anti-aliasing, cross-platform, no shelling out.
- **`IDrawer` is a streaming visitor** — a drawable pushes primitives; a backend turns them into pixels or SVG. One model, multiple outputs.
- **Immutable model** over `ReadOnlyMemory<byte>`; coordinates stay in **twips** until the backend converts to pixels.
- **Tunable error policy** via `RenderOptions` — `Strict` to fail fast, or an `IDiagnosticSink` to collect and keep going.

## Notes

- **Coverage** — shapes, sprites, timelines, morph shapes, buttons, static and edit text, and bitmaps (JPEG and lossless). Gradients, line/fill styles, colour transforms, blend modes, clip masks and filters are honoured.
- **Status** — newer and less battle-tested than the parser; the rendering surface may still evolve.

---

Part of the [ShockwaveFlash](https://github.com/AerafalDev/ShockwaveFlash) project · [MIT](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE) © Aerafal
