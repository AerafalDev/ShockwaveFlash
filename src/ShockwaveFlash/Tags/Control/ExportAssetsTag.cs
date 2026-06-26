using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags.Control;

public sealed class ExportAssetsTag : Tag
{
    public AssetReference[] Assets { get; set; }

    public ExportAssetsTag(TagMetadata metadata, AssetReference[] assets) : base(metadata)
    {
        Assets = assets;
    }

    public static ExportAssetsTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var assetsCount = reader.ReadUInt16();
        var assets = new AssetReference[assetsCount];

        for (var i = 0; i < assets.Length; i++)
            assets[i] = AssetReference.Decode(reader);

        return new ExportAssetsTag(metadata, assets);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16((ushort)Assets.Length);

        foreach (var asset in Assets)
            asset.Encode(writer);
    }
}
