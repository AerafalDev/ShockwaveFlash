using ShockwaveFlash.Types.Text;

namespace ShockwaveFlash.Tags.Text;

public sealed record CsmTextSettingsTag(TagMetadata Metadata, ushort Id, bool UseAdvancedRendering, TextGridFit GridFit, float Thickness, float Sharpness) : Tag(Metadata)
{
    public static CsmTextSettingsTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var flags = reader.ReadUInt8();
        var useAdvancedRendering = (flags & 64) is not 0;
        var gridFit = (TextGridFit)((flags >> 3) & 3);
        var thickness = reader.ReadFloat32();
        var sharpness = reader.ReadFloat32();

        reader.Advance(sizeof(byte));

        return new CsmTextSettingsTag(metadata, id, useAdvancedRendering, gridFit, thickness, sharpness);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);

        var flags = (byte)(((byte)GridFit & 3) << 3);

        if (UseAdvancedRendering)
            flags |= 64;

        writer.WriteUInt8(flags);
        writer.WriteFloat32(Thickness);
        writer.WriteFloat32(Sharpness);
        writer.WriteUInt8(0);
    }
}
