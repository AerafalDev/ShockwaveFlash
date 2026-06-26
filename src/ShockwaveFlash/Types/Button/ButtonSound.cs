using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Types.Button;

public sealed class ButtonSound
{
    public ushort Id { get; set; }

    public SoundInfo SoundInfo { get; set; }

    public ButtonSound(ushort id, SoundInfo soundInfo)
    {
        Id = id;
        SoundInfo = soundInfo;
    }
}
