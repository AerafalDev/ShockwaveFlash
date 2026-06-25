// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

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
