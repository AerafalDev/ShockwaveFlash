// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

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
