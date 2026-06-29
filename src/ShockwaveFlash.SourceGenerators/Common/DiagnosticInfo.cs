using Microsoft.CodeAnalysis;

namespace ShockwaveFlash.SourceGenerators;

internal readonly record struct DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    LocationInfo? Location,
    EquatableArray<string> Arguments)
{
    public Diagnostic ToDiagnostic()
    {
        var arguments = new object[Arguments.Count];

        for (var i = 0; i < Arguments.Count; i++)
            arguments[i] = Arguments[i];

        return Diagnostic.Create(Descriptor, Location?.ToLocation(), arguments);
    }
}
