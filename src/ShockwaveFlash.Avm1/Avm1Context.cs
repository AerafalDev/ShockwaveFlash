using System.Text;

namespace ShockwaveFlash.Avm1;

public readonly struct Avm1Context
{
    public byte Version { get; }

    public Encoding Encoding { get; }

    public Avm1Context(byte version)
    {
        Version = version;
        Encoding = version >= 6 ? Encoding.UTF8 : Encoding.Latin1;
    }
}
