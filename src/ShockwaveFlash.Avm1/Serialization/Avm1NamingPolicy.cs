namespace ShockwaveFlash.Avm1.Serialization;

public abstract class Avm1NamingPolicy
{
    public static Avm1NamingPolicy CamelCase { get; } = new CamelCaseNamingPolicy();

    public abstract string ConvertName(string name);
}
