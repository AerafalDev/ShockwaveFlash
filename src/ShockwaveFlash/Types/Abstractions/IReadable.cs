// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Abstractions;

public interface IReadable<out T>
{
    static abstract T Read(ref SpanReader reader);
}
