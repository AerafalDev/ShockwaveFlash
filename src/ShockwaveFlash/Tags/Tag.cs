using System.Diagnostics;
using ShockwaveFlash.Exceptions;
using ShockwaveFlash.Tags.Abc;
using ShockwaveFlash.Tags.Action;
using ShockwaveFlash.Tags.Bitmap;
using ShockwaveFlash.Tags.Button;
using ShockwaveFlash.Tags.Control;
using ShockwaveFlash.Tags.DisplayList;
using ShockwaveFlash.Tags.Font;
using ShockwaveFlash.Tags.Metadata;
using ShockwaveFlash.Tags.Morph;
using ShockwaveFlash.Tags.Shape;
using ShockwaveFlash.Tags.Sound;
using ShockwaveFlash.Tags.Sprite;
using ShockwaveFlash.Tags.Text;
using ShockwaveFlash.Tags.Video;

namespace ShockwaveFlash.Tags;

[DebuggerDisplay("{Metadata.Code,nq}")]
public abstract record Tag(TagMetadata Metadata)
{
    private const int MaxShortTagLength = 63;
    private const int MaxNestingDepth = 256;

    [ThreadStatic]
    private static int s_depth;

    public abstract void Encode(MemoryWriter writer, byte swfVersion);

    public static void EncodeCollection(MemoryWriter writer, IReadOnlyList<Tag> tags, byte swfVersion)
    {
        var body = new MemoryWriter();

        foreach (var tag in tags)
        {
            body.Reset();
            tag.Encode(body, swfVersion);

            var code = (ushort)tag.Metadata.Code;
            var length = body.Position;

            if (length < MaxShortTagLength)
            {
                writer.WriteUInt16((ushort)((code << 6) | length));
            }
            else
            {
                writer.WriteUInt16((ushort)((code << 6) | MaxShortTagLength));
                writer.WriteInt32(length);
            }

            writer.WriteMemory(body.WrittenMemory);
        }
    }

    public static IReadOnlyList<Tag> DecodeCollection(MemoryReader reader, byte swfVersion)
    {
        if (s_depth >= MaxNestingDepth)
            throw new SwfFormatException($"Tag nesting exceeds the maximum depth of {MaxNestingDepth}.");

        s_depth++;

        try
        {
            var tags = new List<Tag>();

            while (reader.Remaining > 0)
            {
                var codeAndLength = reader.ReadUInt16();
                var code = (ushort)(codeAndLength >> 6);
                var length = codeAndLength & 63;

                if (length is 63)
                    length = reader.ReadInt32();

                var offset = reader.Position;
                var metadata = new TagMetadata((TagCode)code, offset, length);
                var tagReader = new MemoryReader(reader.ReadMemory(length));

                var tag = Decode(tagReader, metadata, swfVersion);

                if (tagReader.Remaining is not 0)
                    throw new SwfFormatException($"Tag length mismatch for {metadata.Code}. Expected {length} bytes, consumed {tagReader.Position}.");

                tags.Add(tag);
            }

            return tags;
        }
        finally
        {
            s_depth--;
        }
    }

    private static Tag Decode(MemoryReader reader, TagMetadata metadata, byte swfVersion)
    {
        return metadata.Code switch
        {
            TagCode.End => new EndTag(metadata),
            TagCode.ShowFrame => new ShowFrameTag(metadata),
            TagCode.DefineShape => DefineShapeTag.Decode(reader, metadata, swfVersion, 1),
            TagCode.PlaceObject => PlaceObjectTag.Decode(reader, metadata),
            TagCode.RemoveObject => RemoveObjectTag.Decode(reader, metadata),
            TagCode.DefineBits => DefineBitsTag.Decode(reader, metadata),
            TagCode.DefineButton => DefineButtonTag.Decode(reader, metadata),
            TagCode.JpegTables => JpegTablesTag.Decode(reader, metadata),
            TagCode.SetBackgroundColor => SetBackgroundColorTag.Decode(reader, metadata),
            TagCode.DefineFont => DefineFontTag.Decode(reader, metadata, swfVersion),
            TagCode.DefineText => DefineTextTag.Decode(reader, metadata),
            TagCode.DoAction => DoActionTag.Decode(reader, metadata),
            TagCode.DefineFontInfo => DefineFontInfoTag.Decode(reader, metadata),
            TagCode.DefineSound => DefineSoundTag.Decode(reader, metadata),
            TagCode.StartSound => StartSoundTag.Decode(reader, metadata),
            TagCode.DefineButtonSound => DefineButtonSoundTag.Decode(reader, metadata),
            TagCode.SoundStreamHead => SoundStreamHeadTag.Decode(reader, metadata),
            TagCode.SoundStreamBlock => SoundStreamBlockTag.Decode(reader, metadata),
            TagCode.DefineBitsLossless => DefineBitsLosslessTag.Decode(reader, metadata),
            TagCode.DefineBitsJpeg2 => DefineBitsJpeg2Tag.Decode(reader, metadata),
            TagCode.DefineShape2 => DefineShapeTag.Decode(reader, metadata, swfVersion, 2),
            TagCode.DefineButtonCxForm => DefineButtonCxFormTag.Decode(reader, metadata),
            TagCode.Protect => ProtectTag.Decode(reader, metadata),
            TagCode.PlaceObject2 => PlaceObject2Tag.Decode(reader, metadata, swfVersion),
            TagCode.RemoveObject2 => RemoveObject2Tag.Decode(reader, metadata),
            TagCode.DefineShape3 => DefineShapeTag.Decode(reader, metadata, swfVersion, 3),
            TagCode.DefineText2 => DefineText2Tag.Decode(reader, metadata),
            TagCode.DefineButton2 => DefineButton2Tag.Decode(reader, metadata),
            TagCode.DefineBitsJpeg3 => DefineBitsJpeg3Tag.Decode(reader, metadata),
            TagCode.DefineBitsLossless2 => DefineBitsLossless2Tag.Decode(reader, metadata),
            TagCode.DefineEditText => DefineEditTextTag.Decode(reader, metadata),
            TagCode.DefineSprite => DefineSpriteTag.Decode(reader, metadata, swfVersion),
            TagCode.NameCharacter => NameCharacterTag.Decode(reader, metadata),
            TagCode.ProductInfo => ProductInfoTag.Decode(reader, metadata),
            TagCode.FrameLabel => FrameLabelTag.Decode(reader, metadata, swfVersion),
            TagCode.SoundStreamHead2 => SoundStreamHead2Tag.Decode(reader, metadata),
            TagCode.DefineMorphShape => DefineMorphShapeTag.Decode(reader, metadata, swfVersion),
            TagCode.DefineFont2 => DefineFont2Tag.Decode(reader, metadata, swfVersion),
            TagCode.ExportAssets => ExportAssetsTag.Decode(reader, metadata),
            TagCode.ImportAssets => ImportAssetsTag.Decode(reader, metadata),
            TagCode.EnableDebugger => EnableDebuggerTag.Decode(reader, metadata),
            TagCode.DoInitAction => DoInitActionTag.Decode(reader, metadata),
            TagCode.DefineVideoStream => DefineVideoStreamTag.Decode(reader, metadata),
            TagCode.VideoFrame => VideoFrameTag.Decode(reader, metadata),
            TagCode.DefineFontInfo2 => DefineFontInfo2Tag.Decode(reader, metadata),
            TagCode.DebugId => DebugIdTag.Decode(reader, metadata),
            TagCode.EnableDebugger2 => EnableDebugger2Tag.Decode(reader, metadata),
            TagCode.ScriptLimits => ScriptLimitsTag.Decode(reader, metadata),
            TagCode.SetTabIndex => SetTabIndexTag.Decode(reader, metadata),
            TagCode.FileAttributes => FileAttributesTag.Decode(reader, metadata),
            TagCode.PlaceObject3 => PlaceObject3Tag.Decode(reader, metadata, swfVersion),
            TagCode.ImportAssets2 => ImportAssets2Tag.Decode(reader, metadata),
            TagCode.DoAbc => DoAbcTag.Decode(reader, metadata),
            TagCode.DefineFontAlignZones => DefineFontAlignZonesTag.Decode(reader, metadata),
            TagCode.CsmTextSettings => CsmTextSettingsTag.Decode(reader, metadata),
            TagCode.DefineFont3 => DefineFont3Tag.Decode(reader, metadata, swfVersion),
            TagCode.SymbolClass => SymbolClassTag.Decode(reader, metadata),
            TagCode.Metadata => MetadataTag.Decode(reader, metadata),
            TagCode.DefineScalingGrid => DefineScalingGridTag.Decode(reader, metadata),
            TagCode.DoAbc2 => DoAbc2Tag.Decode(reader, metadata),
            TagCode.DefineShape4 => DefineShapeTag.Decode(reader, metadata, swfVersion, 4),
            TagCode.DefineMorphShape2 => DefineMorphShape2Tag.Decode(reader, metadata, swfVersion),
            TagCode.DefineSceneAndFrameLabelData => DefineSceneAndFrameLabelDataTag.Decode(reader, metadata),
            TagCode.DefineBinaryData => DefineBinaryDataTag.Decode(reader, metadata),
            TagCode.DefineFontName => DefineFontNameTag.Decode(reader, metadata),
            TagCode.StartSound2 => StartSound2Tag.Decode(reader, metadata),
            TagCode.DefineBitsJpeg4 => DefineBitsJpeg4Tag.Decode(reader, metadata),
            TagCode.DefineFont4 => DefineFont4Tag.Decode(reader, metadata),
            TagCode.EnableTelemetry => EnableTelemetryTag.Decode(reader, metadata),
            _ => UnknownTag.Decode(reader, metadata)
        };
    }
}
