namespace ShockwaveFlash.SourceGenerators.Avm1.Models;

internal readonly record struct Avm1RegistrationModel(
    string FullyQualifiedName,
    string AccessorName,
    EquatableArray<string> BindingPath,
    Avm1TypeModel? ObjectModel,
    string? DiscriminatorName,
    EquatableArray<Avm1DerivedTypeModel> Derived);
