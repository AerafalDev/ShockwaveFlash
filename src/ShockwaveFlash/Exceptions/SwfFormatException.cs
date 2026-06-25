namespace ShockwaveFlash.Exceptions;

public sealed class SwfFormatException : SwfException
{
    public SwfFormatException(string message)
        : base(message)
    {
    }

    public SwfFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
