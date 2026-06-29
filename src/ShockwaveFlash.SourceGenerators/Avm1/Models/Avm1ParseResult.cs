namespace ShockwaveFlash.SourceGenerators.Avm1.Models;

internal readonly record struct Avm1ParseResult(
    Avm1TypeModel? Model,
    EquatableArray<DiagnosticInfo> Diagnostics);
