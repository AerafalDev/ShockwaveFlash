using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags.Control;

public sealed class ImportAssets2Tag : Tag
{
    public string Url { get; set; }

    public AssetReference[] Assets { get; set; }

    public ImportAssets2Tag(TagMetadata metadata, string url, AssetReference[] assets) : base(metadata)
    {
        Url = url;
        Assets = assets;
    }

    public static ImportAssets2Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var url = reader.ReadNullTerminatedString();

        reader.Advance(sizeof(ushort));

        var numAssets = reader.ReadUInt16();
        var assets = new AssetReference[numAssets];

        for (var i = 0; i < numAssets; i++)
            assets[i] = AssetReference.Decode(reader);

        return new ImportAssets2Tag(metadata, url, assets);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteNullTerminatedString(Url);
        writer.WriteUInt16(0);
        writer.WriteUInt16((ushort)Assets.Length);

        foreach (var asset in Assets)
            asset.Encode(writer);
    }
}
