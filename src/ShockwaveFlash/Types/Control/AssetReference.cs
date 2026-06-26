namespace ShockwaveFlash.Types.Control;

public sealed class AssetReference
{
    public ushort Id { get; set; }

    public string Name { get; set; }

    public AssetReference(ushort id, string name)
    {
        Id = id;
        Name = name;
    }

    public static AssetReference Decode(MemoryReader reader)
    {
        return new AssetReference(reader.ReadUInt16(), reader.ReadNullTerminatedString());
    }

    public void Encode(MemoryWriter writer)
    {
        writer.WriteUInt16(Id);
        writer.WriteNullTerminatedString(Name);
    }
}
