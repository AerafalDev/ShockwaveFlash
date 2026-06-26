namespace ShockwaveFlash.Types.Control;

public sealed class FrameLabel
{
    public uint FrameNum { get; set; }

    public string Label { get; set; }

    public FrameLabel(uint frameNum, string label)
    {
        FrameNum = frameNum;
        Label = label;
    }
}
