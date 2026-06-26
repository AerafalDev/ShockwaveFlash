using ShockwaveFlash.Exceptions;

namespace ShockwaveFlash.Avm1.Exceptions;

public sealed class Avm1UnsupportedActionException : SwfException
{
    public ActionOpcode Opcode { get; }

    public Avm1UnsupportedActionException(ActionOpcode opcode)
        : base($"AVM1 evaluation encountered the unsupported action {opcode}.")
    {
        Opcode = opcode;
    }
}
