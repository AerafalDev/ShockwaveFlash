namespace ShockwaveFlash.SourceGenerators.Avm1.Models;

internal readonly record struct Avm1RegistrationModel(
    Avm1TypeModel TypeModel,
    EquatableArray<string> BindingPath,
    string AccessorName);
