namespace ShockwaveFlash.Avm1.Serialization;

[Flags]
public enum Avm1NumberHandling
{
    Strict = 0,
    AllowReadingFromString = 1,
    WriteAsString = 2,
    AllowNamedFloatingPointLiterals = 4,
}
