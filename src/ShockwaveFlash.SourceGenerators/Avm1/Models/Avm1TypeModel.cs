namespace ShockwaveFlash.SourceGenerators.Avm1.Models;

internal readonly record struct Avm1TypeModel(
    string? Namespace,
    EquatableArray<string> ContainingTypes,
    string TypeKeyword,
    string TypeName,
    string FullyQualifiedName,
    string HintName,
    string? GlobalName,
    ConstructionKind Construction,
    EquatableArray<string> ConstructorParameterOrder,
    EquatableArray<Avm1MemberModel> Members);
