using ShockwaveFlash.Types.Morph;

namespace ShockwaveFlash.Tags.Morph;

public sealed class DefineMorphShape2Tag : DefineMorphShapeTag
{
    public DefineMorphShape2Tag(TagMetadata metadata, ushort id, DefineMorphShapeFlags flags, MorphShape start, MorphShape end)
        : base(metadata, id, flags, start, end)
    {
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        Encode(writer, swfVersion, 2);
    }
}
