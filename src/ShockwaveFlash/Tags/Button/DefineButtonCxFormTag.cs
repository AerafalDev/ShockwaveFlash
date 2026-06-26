using ShockwaveFlash.Types;

namespace ShockwaveFlash.Tags.Button;

public sealed class DefineButtonCxFormTag : Tag
{
    public ushort Id { get; set; }

    public IReadOnlyList<ColorTransform> ColorTransforms { get; set; }

    public DefineButtonCxFormTag(TagMetadata metadata, ushort id, IReadOnlyList<ColorTransform> colorTransforms) : base(metadata)
    {
        Id = id;
        ColorTransforms = colorTransforms;
    }

    public static DefineButtonCxFormTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var colorTransforms = new List<ColorTransform>();

        while (reader.Remaining > 0)
            colorTransforms.Add(ColorTransform.DecodeRgb(reader));

        return new DefineButtonCxFormTag(metadata, id, colorTransforms);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);

        foreach (var colorTransform in ColorTransforms)
            colorTransform.EncodeRgb(writer);
    }
}
