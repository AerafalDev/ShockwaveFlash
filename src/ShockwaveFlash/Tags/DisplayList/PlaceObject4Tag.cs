namespace ShockwaveFlash.Tags.DisplayList;

public sealed record PlaceObject4Tag(PlaceObject3Tag Placement, ReadOnlyMemory<byte> AmfData) : Tag(Placement.Metadata)
{
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
