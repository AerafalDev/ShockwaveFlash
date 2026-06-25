namespace ShockwaveFlash.Exceptions;

public sealed class SwfCompressionException : SwfException
{
    public SwfCompressionException(string message)
        : base(message)
    {
    }

    public SwfCompressionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
