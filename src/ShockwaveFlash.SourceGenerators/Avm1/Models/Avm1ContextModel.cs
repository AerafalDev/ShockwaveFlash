namespace ShockwaveFlash.SourceGenerators.Avm1.Models;

internal readonly record struct Avm1ContextModel(
    string? Namespace,
    EquatableArray<string> ContainingTypes,
    string TypeName,
    string Accessibility,
    string HintName,
    EquatableArray<Avm1RegistrationModel> Registrations);
