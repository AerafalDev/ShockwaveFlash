using ShockwaveFlash.Exceptions;

namespace ShockwaveFlash.Avm1.Exceptions;

public sealed class Avm1SerializationException : SwfException
{
    public Avm1SerializationException(string message)
        : base(message)
    {
    }
}
