// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

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
