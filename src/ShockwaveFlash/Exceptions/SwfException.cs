namespace ShockwaveFlash.Exceptions;

public class SwfException : Exception
{
    public SwfException(string message)
        : base(message)
    {
    }

    public SwfException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
