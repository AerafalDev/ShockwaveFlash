namespace ShockwaveFlash.Types.DisplayList;

public abstract class PlaceObjectAction
{
    public sealed class PlaceObjectActionPlace : PlaceObjectAction
    {
        public ushort Id { get; set; }

        public PlaceObjectActionPlace(ushort id)
        {
            Id = id;
        }
    }

    public sealed class PlaceObjectActionReplace : PlaceObjectAction
    {
        public ushort Id { get; set; }

        public PlaceObjectActionReplace(ushort id)
        {
            Id = id;
        }
    }

    public sealed class PlaceObjectActionModify : PlaceObjectAction
    {
    }

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
