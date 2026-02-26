// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;

namespace ShockwaveFlash.Actions.Avm1.Swf1;

public sealed record ActionGetURL(string Url, string Target) : Action(ActionOpcode.GetURL)
{
    public static ActionGetURL Decode(ref SpanReader reader, Encoding encoding)
    {
        return new ActionGetURL(reader.ReadNullTerminatedString(encoding), reader.ReadNullTerminatedString(encoding));
    }
}
