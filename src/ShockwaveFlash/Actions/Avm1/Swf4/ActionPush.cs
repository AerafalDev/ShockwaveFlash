// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Actions.Types;

namespace ShockwaveFlash.Actions.Avm1.Swf4;

public sealed record ActionPush(IReadOnlyList<PushValue> PushValues) : Action(ActionOpcode.Push);
