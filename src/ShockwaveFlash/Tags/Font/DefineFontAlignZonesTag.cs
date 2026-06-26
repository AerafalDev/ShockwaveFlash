using ShockwaveFlash.Types.Font;

namespace ShockwaveFlash.Tags.Font;

public sealed class DefineFontAlignZonesTag : Tag
{
    public ushort Id { get; set; }

    public FontThickness Thickness { get; set; }

    public IReadOnlyList<FontAlignZone> Zones { get; set; }

    public DefineFontAlignZonesTag(TagMetadata metadata, ushort id, FontThickness thickness, IReadOnlyList<FontAlignZone> zones) : base(metadata)
    {
        Id = id;
        Thickness = thickness;
        Zones = zones;
    }

    public static DefineFontAlignZonesTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var thickness = (FontThickness)(reader.ReadUInt8() >> 6);
        var zones = new List<FontAlignZone>();

        while (reader.Remaining > 0)
            zones.Add(FontAlignZone.Decode(reader));

        return new DefineFontAlignZonesTag(metadata, id, thickness, zones);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteUInt8((byte)((byte)Thickness << 6));

        foreach (var zone in Zones)
            zone.Encode(writer);
    }
}
