using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Text;

namespace ShockwaveFlash.Tags.Text;

public sealed class DefineText2Tag : DefineTextTag
{
    public DefineText2Tag(TagMetadata metadata, ushort id, Rectangle bounds, Matrix matrix, IReadOnlyList<TextRecord> records)
        : base(metadata, id, bounds, matrix, records)
    {
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        EncodeBody(writer, 2);
    }
}
