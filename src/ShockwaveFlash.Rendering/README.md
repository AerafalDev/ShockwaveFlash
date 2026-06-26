# ShockwaveFlash.Rendering

[![NuGet](https://img.shields.io/nuget/v/ShockwaveFlash.Rendering.svg)](https://www.nuget.org/packages/ShockwaveFlash.Rendering)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE)

Render SWF characters — shapes, sprites, timelines, images — parsed by [**ShockwaveFlash**](https://www.nuget.org/packages/ShockwaveFlash) to **SVG** and to raster images (**PNG/JPEG/WebP/GIF**, still or animated) through a native, cross-platform **Skia** backend. No external tools (no Inkscape/rsvg/ImageMagick).

> **Status: work in progress.** The immutable drawable model and the rendering contracts are in place. The processors (tags → model), the Skia and SVG backends, image decoding, text and the timeline are being built.

## Design

A clean layering, [inspired](https://github.com/Arakne/ArakneSwf) by Arakne-Swf but with the rasterization pulled in-process via Skia:

```
ShockwaveFlash            parser / writer
   └─ ShockwaveFlash.Rendering
        ├─ Model/      immutable drawable data (Shapes, Images, …)
        ├─ Processing/ tags → model (ShapeProcessor, TimelineProcessor, …)
        ├─ Scene/      display list (Frame, FrameObject), blend modes
        ├─ Drawing/    IDrawer visitor + backends
        │     ├─ Skia   raster (default)
        │     └─ Svg    vector export
        └─ Output/     PNG/JPEG/WebP/GIF encoders + animation
```

Key choices:
- **Skia is the native rasterizer** (exact gradients, blend modes, filters, anti-aliasing) — cross-platform, no shelling out.
- **`IDrawer` is a streaming visitor**: a drawable pushes primitives, a backend turns them into SVG or pixels. One model, multiple outputs.
- **Immutable model** over `ReadOnlyMemory<byte>`; coordinates in **twips** until the backend converts to pixels.
- **Tunable error policy** via `RenderOptions` (`Strict` + an `IDiagnosticSink`) — fail-fast or fail-safe.

> On headless Linux, add `SkiaSharp.NativeAssets.Linux.NoDependencies`.

## License

[MIT](https://github.com/AerafalDev/ShockwaveFlash/blob/main/LICENSE) © Aerafal
