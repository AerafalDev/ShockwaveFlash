namespace ShockwaveFlash.Tags.DisplayList;

public sealed class PlaceObject4Tag : Tag
{
    public PlaceObject3Tag Placement { get; set; }

    public ReadOnlyMemory<byte> AmfData { get; set; }

    public PlaceObject4Tag(PlaceObject3Tag placement, ReadOnlyMemory<byte> amfData) : base(placement.Metadata)
    {
        Placement = placement;
        AmfData = amfData;
    }

    public static PlaceObject4Tag Decode(MemoryReader reader, TagMetadata metadata, byte swfVersion)
    {
        var placement = PlaceObject3Tag.Decode(reader, metadata, swfVersion);
        var amfData = reader.ReadMemoryToEnd();

        return new PlaceObject4Tag(placement, amfData);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        Placement.Encode(writer, swfVersion);
        writer.WriteMemory(AmfData);
    }
}
