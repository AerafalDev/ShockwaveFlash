// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.DisplayList;

public abstract record PlaceObjectAction
{
    public sealed record PlaceObjectActionPlace(ushort Id) : PlaceObjectAction;

    public sealed record PlaceObjectActionReplace(ushort Id) : PlaceObjectAction;

    public sealed record PlaceObjectActionModify : PlaceObjectAction;

    public static PlaceObjectAction Place(ushort id)
    {
        return new PlaceObjectActionPlace(id);
    }

    public static PlaceObjectAction Replace(ushort id)
    {
        return new PlaceObjectActionReplace(id);
    }

    public static PlaceObjectAction Modify()
    {
        return new PlaceObjectActionModify();
    }
}
