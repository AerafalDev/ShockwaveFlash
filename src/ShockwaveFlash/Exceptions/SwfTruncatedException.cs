// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Exceptions;

public sealed class SwfTruncatedException : SwfException
{
    public SwfTruncatedException(string message)
        : base(message)
    {
    }
}
