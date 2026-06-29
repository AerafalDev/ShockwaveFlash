namespace ShockwaveFlash.Avm1.Serialization;

internal sealed class CamelCaseNamingPolicy : Avm1NamingPolicy
{
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
            return name;

        var chars = name.ToCharArray();
        chars[0] = char.ToLowerInvariant(chars[0]);
        return new string(chars);
    }
}
