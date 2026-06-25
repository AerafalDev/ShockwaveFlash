namespace ShockwaveFlash.Exceptions;

public sealed class SwfUnsupportedException : SwfException
{
    public SwfUnsupportedException(string message)
        : base(message)
    {
    }
}
