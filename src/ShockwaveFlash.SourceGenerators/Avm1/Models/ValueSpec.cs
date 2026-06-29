namespace ShockwaveFlash.SourceGenerators.Avm1.Models;

internal sealed record ValueSpec(
    ValueKind Kind,
    ConvKind Conv,
    string TypeFqn,
    bool IsValueType,
    ValueSpec? Element);
