namespace ShockwaveFlash.Rendering.Diagnostics;

public enum RenderSeverity
{
    Info,
    Warning,
    Error
}

public readonly record struct RenderDiagnostic(RenderSeverity Severity, string Message);

public interface IDiagnosticSink
{
    void Report(RenderDiagnostic diagnostic);
}
