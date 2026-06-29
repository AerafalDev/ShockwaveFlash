namespace ShockwaveFlash.SourceGenerators.Avm1.Models;

internal readonly record struct Avm1ContextParseResult(
    Avm1ContextModel? Model,
    EquatableArray<DiagnosticInfo> Diagnostics);
