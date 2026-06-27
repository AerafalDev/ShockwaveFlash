using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Font;

namespace ShockwaveFlash.Tags.Font;

public sealed class DefineFont3Tag : DefineFont2Tag
{
    public DefineFont3Tag(TagMetadata metadata, ushort id, string name, Language language, FontLayout? layout, FontGlyph[] glyphs, FontFlags flags)
        : base(metadata, id, name, language, layout, glyphs, flags)
    {
    }
}
