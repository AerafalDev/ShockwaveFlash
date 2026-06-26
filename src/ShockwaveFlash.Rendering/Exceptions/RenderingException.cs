using ShockwaveFlash.Exceptions;

namespace ShockwaveFlash.Rendering.Exceptions;

public class RenderingException : SwfException
{
    public RenderingException(string message)
        : base(message)
    {
    }

    public RenderingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
