// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;

namespace ShockwaveFlash.Actions.Avm1.Swf5;

public sealed record ActionDefineFunction(string Name, IReadOnlyList<string> Parameters, int CodeSize) : Action(ActionOpcode.DefineFunction)
{
    public static ActionDefineFunction Decode(ref SpanReader reader, Encoding encoding)
    {
        var name = reader.ReadNullTerminatedString(encoding);
        var numParams = reader.ReadUInt16();
        var parameters = new string[numParams];

        for (var i = 0; i < numParams; i++)
            parameters[i] = reader.ReadNullTerminatedString(encoding);

        var codeSize = reader.ReadUInt16();

        return new ActionDefineFunction(name, parameters, codeSize);
    }
}
