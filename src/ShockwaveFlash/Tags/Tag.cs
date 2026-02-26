// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
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
    public static IReadOnlyList<Tag> DecodeCollection(ref SpanReader reader, byte swfVersion)
    {
        var tags = new List<Tag>();

        while (reader.Remaining > 0)
        {
            var codeAndLength = reader.ReadUInt16();
            var code = (ushort)(codeAndLength >> 6);
            var length = codeAndLength & 63;

            if (length is 63)
                length = reader.ReadInt32();

            if (!Enum.IsDefined(typeof(TagCode), code))
                throw new NotSupportedException($"Tag code {code} is not supported.");

            var offset = reader.Position;
            var metadata = new TagMetadata((TagCode)code, offset, length);
            var tagReader = new SpanReader(reader.ReadSpan(length));

            var tag = Decode(ref tagReader, metadata, swfVersion);

            if (tagReader.Remaining is not 0)
                throw new Exception($"Tag length mismatch. Expected {length} bytes, got {tagReader.Position}.");

            tags.Add(tag);
        }

        return tags;
    }

    private static Tag Decode(ref SpanReader reader, TagMetadata metadata, byte swfVersion)
    {
        return metadata.Code switch
        {
            TagCode.End => new EndTag(metadata),
            TagCode.ShowFrame => new ShowFrameTag(metadata),
            TagCode.DefineShape => DefineShapeTag.Decode(ref reader, metadata, swfVersion, 1),
            TagCode.PlaceObject => PlaceObjectTag.Decode(ref reader, metadata),
            TagCode.RemoveObject => RemoveObjectTag.Decode(ref reader, metadata),
            TagCode.DefineBits => DefineBitsTag.Decode(ref reader, metadata),
            TagCode.DefineButton => DefineButtonTag.Decode(ref reader, metadata),
            TagCode.JpegTables => JpegTablesTag.Decode(ref reader, metadata),
            TagCode.SetBackgroundColor => SetBackgroundColorTag.Decode(ref reader, metadata),
            TagCode.DefineFont => DefineFontTag.Decode(ref reader, metadata, swfVersion),
            TagCode.DefineText => DefineTextTag.Decode(ref reader, metadata),
            TagCode.DoAction => DoActionTag.Decode(ref reader, metadata),
            TagCode.DefineFontInfo => DefineFontInfoTag.Decode(ref reader, metadata),
            TagCode.DefineSound => DefineSoundTag.Decode(ref reader, metadata),
            TagCode.StartSound => StartSoundTag.Decode(ref reader, metadata),
            TagCode.DefineButtonSound => DefineButtonSoundTag.Decode(ref reader, metadata),
            TagCode.SoundStreamHead => SoundStreamHeadTag.Decode(ref reader, metadata),
            TagCode.SoundStreamBlock => SoundStreamBlockTag.Decode(ref reader, metadata),
            TagCode.DefineBitsLossless => DefineBitsLosslessTag.Decode(ref reader, metadata),
            TagCode.DefineBitsJpeg2 => DefineBitsJpeg2Tag.Decode(ref reader, metadata),
            TagCode.DefineShape2 => DefineShapeTag.Decode(ref reader, metadata, swfVersion, 2),
            TagCode.DefineButtonCxForm => DefineButtonCxFormTag.Decode(ref reader, metadata),
            TagCode.Protect => ProtectTag.Decode(ref reader, metadata),
            TagCode.PlaceObject2 => PlaceObject2Tag.Decode(ref reader, metadata, swfVersion),
            TagCode.RemoveObject2 => RemoveObject2Tag.Decode(ref reader, metadata),
            TagCode.DefineShape3 => DefineShapeTag.Decode(ref reader, metadata, swfVersion, 3),
            TagCode.DefineText2 => DefineText2Tag.Decode(ref reader, metadata),
            TagCode.DefineButton2 => DefineButton2Tag.Decode(ref reader, metadata),
            TagCode.DefineBitsJpeg3 => DefineBitsJpeg3Tag.Decode(ref reader, metadata),
            TagCode.DefineBitsLossless2 => DefineBitsLossless2Tag.Decode(ref reader, metadata),
            TagCode.DefineEditText => DefineEditTextTag.Decode(ref reader, metadata),
            TagCode.DefineSprite => DefineSpriteTag.Decode(ref reader, metadata, swfVersion),
            TagCode.NameCharacter => NameCharacterTag.Decode(ref reader, metadata),
            TagCode.ProductInfo => ProductInfoTag.Decode(ref reader, metadata),
            TagCode.FrameLabel => FrameLabelTag.Decode(ref reader, metadata, swfVersion),
            TagCode.SoundStreamHead2 => SoundStreamHead2Tag.Decode(ref reader, metadata),
            TagCode.DefineMorphShape => DefineMorphShapeTag.Decode(ref reader, metadata, swfVersion),
            TagCode.DefineFont2 => DefineFont2Tag.Decode(ref reader, metadata, swfVersion),
            TagCode.ExportAssets => ExportAssetsTag.Decode(ref reader, metadata),
            TagCode.ImportAssets => ImportAssetsTag.Decode(ref reader, metadata),
            TagCode.EnableDebugger => EnableDebuggerTag.Decode(ref reader, metadata),
            TagCode.DoInitAction => DoInitActionTag.Decode(ref reader, metadata),
            TagCode.DefineVideoStream => DefineVideoStreamTag.Decode(ref reader, metadata),
            TagCode.VideoFrame => VideoFrameTag.Decode(ref reader, metadata),
            TagCode.DefineFontInfo2 => DefineFontInfo2Tag.Decode(ref reader, metadata),
            TagCode.DebugId => DebugIdTag.Decode(ref reader, metadata),
            TagCode.EnableDebugger2 => EnableDebugger2Tag.Decode(ref reader, metadata),
            TagCode.ScriptLimits => ScriptLimitsTag.Decode(ref reader, metadata),
            TagCode.SetTabIndex => SetTabIndexTag.Decode(ref reader, metadata),
            TagCode.FileAttributes => FileAttributesTag.Decode(ref reader, metadata),
            TagCode.PlaceObject3 => PlaceObject3Tag.Decode(ref reader, metadata, swfVersion),
            TagCode.ImportAssets2 => ImportAssets2Tag.Decode(ref reader, metadata),
            TagCode.DoAbc => DoAbcTag.Decode(ref reader, metadata),
            TagCode.DefineFontAlignZones => DefineFontAlignZonesTag.Decode(ref reader, metadata),
            TagCode.CsmTextSettings => CsmTextSettingsTag.Decode(ref reader, metadata),
            TagCode.DefineFont3 => DefineFont3Tag.Decode(ref reader, metadata, swfVersion),
            TagCode.SymbolClass => SymbolClassTag.Decode(ref reader, metadata),
            TagCode.Metadata => MetadataTag.Decode(ref reader, metadata),
            TagCode.DefineScalingGrid => DefineScalingGridTag.Decode(ref reader, metadata),
            TagCode.DoAbc2 => DoAbc2Tag.Decode(ref reader, metadata),
            TagCode.DefineShape4 => DefineShapeTag.Decode(ref reader, metadata, swfVersion, 4),
            TagCode.DefineMorphShape2 => DefineMorphShape2Tag.Decode(ref reader, metadata, swfVersion),
            TagCode.DefineSceneAndFrameLabelData => DefineSceneAndFrameLabelDataTag.Decode(ref reader, metadata),
            TagCode.DefineBinaryData => DefineBinaryDataTag.Decode(ref reader, metadata),
            TagCode.DefineFontName => DefineFontNameTag.Decode(ref reader, metadata),
            TagCode.StartSound2 => StartSound2Tag.Decode(ref reader, metadata),
            TagCode.DefineBitsJpeg4 => DefineBitsJpeg4Tag.Decode(ref reader, metadata),
            TagCode.DefineFont4 => DefineFont4Tag.Decode(ref reader, metadata),
            TagCode.EnableTelemetry => EnableTelemetryTag.Decode(ref reader, metadata),
            _ => throw new NotSupportedException($"Tag code {metadata.Code} is not supported.")
        };
    }
}
