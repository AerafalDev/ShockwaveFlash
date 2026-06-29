using Microsoft.CodeAnalysis;

namespace ShockwaveFlash.SourceGenerators.Avm1;

internal static class Avm1Diagnostics
{
    private const string Category = "Usage";
    private const string HelpRoot = "https://aerafaldev.github.io/ShockwaveFlash/docs/serialization/diagnostics#";

    public static readonly DiagnosticDescriptor UnsupportedMemberType = new(
        "AVM1002",
        "Unsupported Avm1 member type",
        "Member '{0}.{1}' has type '{2}' which the AVM1 serializer cannot map; mark it with [Avm1Ignore], attach an [Avm1Converter], or use a supported type",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpRoot + "avm1002");

    public static readonly DiagnosticDescriptor NoAccessibleConstructor = new(
        "AVM1003",
        "No usable constructor for Avm1 type",
        "Type '{0}' has no accessible parameterless constructor and no single constructor whose parameters all match (de)serialized members",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpRoot + "avm1003");

    public static readonly DiagnosticDescriptor DuplicateMemberKey = new(
        "AVM1004",
        "Duplicate Avm1 member key",
        "Members '{1}' and '{2}' on type '{0}' both map to the AVM1 key '{3}'; keys must be unique",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpRoot + "avm1004");

    public static readonly DiagnosticDescriptor ContainingTypeMustBePartial = new(
        "AVM1005",
        "Containing type must be partial",
        "Type '{0}' is nested in '{1}' which is not declared 'partial'; every containing type must be partial for generation to succeed",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpRoot + "avm1005");

    public static readonly DiagnosticDescriptor UnsupportedDeclaration = new(
        "AVM1006",
        "Unsupported Avm1 declaration",
        "Type '{0}' cannot be made AVM1 serializable; generic types and ref-like types are not supported",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpRoot + "avm1006");

    public static readonly DiagnosticDescriptor ContextMustBePartial = new(
        "AVM1007",
        "Avm1 serializer context must be partial",
        "Context '{0}' is annotated with [Avm1Serializable] but is not declared 'partial'; the generator cannot complete the Avm1SerializerContext",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpRoot + "avm1007");
}
