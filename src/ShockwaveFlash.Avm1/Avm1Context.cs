using System.Text;

namespace ShockwaveFlash.Avm1;

public readonly struct Avm1Context
{
    private static readonly Encoding s_strictUtf8 = new UTF8Encoding(false, throwOnInvalidBytes: true);

    public byte Version { get; }

    public Encoding Encoding { get; }

    public bool Strict { get; }

    public Avm1Context(byte version, bool strict = false)
    {
        Version = version;
        Encoding = version >= 6 ? (strict ? s_strictUtf8 : Encoding.UTF8) : Encoding.Latin1;
        Strict = strict;
    }
}
