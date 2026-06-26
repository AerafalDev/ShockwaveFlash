namespace ShockwaveFlash.Types.Control;

public sealed class SceneOffset
{
    public uint Offset { get; set; }

    public string Name { get; set; }

    public SceneOffset(uint offset, string name)
    {
        Offset = offset;
        Name = name;
    }
}
