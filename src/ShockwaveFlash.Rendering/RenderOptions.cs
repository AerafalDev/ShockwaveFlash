using ShockwaveFlash.Rendering.Diagnostics;

namespace ShockwaveFlash.Rendering;

public sealed record RenderOptions
{
    public bool Strict { get; init; }

    public IDiagnosticSink? Diagnostics { get; init; }
}
