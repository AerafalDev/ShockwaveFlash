namespace ShockwaveFlash.SourceGenerators.Avm1.Models;

internal readonly record struct Avm1MemberModel(
    string CSharpName,
    string Avm1Key,
    string DeclaredType,
    ValueSpec Value,
    bool MemberNullable,
    bool ThrowIfMissing,
    bool IsConstructorParameter,
    bool IsSettable,
    string? ConverterTypeFqn,
    int Order,
    bool IsExtensionData,
    bool IsKeyExplicit);
