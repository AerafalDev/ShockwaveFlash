namespace ShockwaveFlash.Exceptions;

public sealed class SwfTruncatedException : SwfException
{
    public SwfTruncatedException(string message)
        : base(message)
    {
    }
}
