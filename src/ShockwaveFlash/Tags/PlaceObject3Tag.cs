// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types;
using ShockwaveFlash.Types.DisplayList;
using ShockwaveFlash.Types.Filter;

namespace ShockwaveFlash.Tags;

public sealed record PlaceObject3Tag(
    TagMetadata Metadata,
    PlaceObjectAction Action,
    ushort Depth,
    string? ClassName,
    Matrix? Matrix,
    ColorTransform? ColorTransform,
    PlaceObjectFlags Flags,
    ushort? Ratio,
    string? Name,
    ushort? ClipDepth,
    Filter[]? Filters,
    BlendMode? BlendMode,
    bool? IsBitmapCached,
    bool? IsVisible,
    Color? BackgroundColor,
    IReadOnlyList<ClipAction>? ClipActions) : Tag(Metadata)
{
    public bool Move =>
        Flags.HasFlag(PlaceObjectFlags.Move);

    public bool HasCharacter =>
        Flags.HasFlag(PlaceObjectFlags.HasCharacter);

    public bool HasMatrix =>
        Flags.HasFlag(PlaceObjectFlags.HasMatrix);

    public bool HasColorTransform =>
        Flags.HasFlag(PlaceObjectFlags.HasColorTransform);

    public bool HasRatio =>
        Flags.HasFlag(PlaceObjectFlags.HasRatio);

    public bool HasName =>
        Flags.HasFlag(PlaceObjectFlags.HasName);

    public bool HasClipDepth =>
        Flags.HasFlag(PlaceObjectFlags.HasClipDepth);

    public bool HasClipActions =>
        Flags.HasFlag(PlaceObjectFlags.HasClipActions);

    public bool HasFilterList =>
        Flags.HasFlag(PlaceObjectFlags.HasFilterList);

    public bool HasBlendMode =>
        Flags.HasFlag(PlaceObjectFlags.HasBlendMode);

    public bool HasCacheAsBitmap =>
        Flags.HasFlag(PlaceObjectFlags.HasCacheAsBitmap);

    public bool HasClassName =>
        Flags.HasFlag(PlaceObjectFlags.HasClassName);

    public bool HasImage =>
        Flags.HasFlag(PlaceObjectFlags.HasImage);

    public bool HasVisible =>
        Flags.HasFlag(PlaceObjectFlags.HasVisible);

    public bool OpaqueBackground =>
        Flags.HasFlag(PlaceObjectFlags.OpaqueBackground);

    public static PlaceObject3Tag Decode(ref SpanReader reader, TagMetadata metadata, byte swfVersion)
    {
        var flags = (PlaceObjectFlags)reader.ReadUInt16();
        var depth = reader.ReadUInt16();

        var hasMove = flags.HasFlag(PlaceObjectFlags.Move);
        var hasImage = flags.HasFlag(PlaceObjectFlags.HasImage);
        var hasid = flags.HasFlag(PlaceObjectFlags.HasCharacter);
        var hasClassName = flags.HasFlag(PlaceObjectFlags.HasClassName) || (hasImage && !hasid);

        var className = hasClassName ? reader.ReadNullTerminatedString() : null;

        var action = (hasMove, hasid) switch
        {
            (true, false) => PlaceObjectAction.Modify(),
            (false, true) => PlaceObjectAction.Place(reader.ReadUInt16()),
            (true, true) => PlaceObjectAction.Replace(reader.ReadUInt16()),
            _ => throw new NotSupportedException("Invalid PlaceObject3Tag action combination.")
        };

        Matrix? matrix = flags.HasFlag(PlaceObjectFlags.HasMatrix)
            ? Types.Matrix.Decode(ref reader)
            : null;

        ColorTransform? colorTransform = flags.HasFlag(PlaceObjectFlags.HasColorTransform)
            ? Types.ColorTransform.DecodeRgba(ref reader)
            : null;

        ushort? ratio = flags.HasFlag(PlaceObjectFlags.HasRatio)
            ? reader.ReadUInt16()
            : null;

        var name = flags.HasFlag(PlaceObjectFlags.HasName)
            ? reader.ReadNullTerminatedString()
            : null;

        ushort? clipDepth = flags.HasFlag(PlaceObjectFlags.HasClipDepth)
            ? reader.ReadUInt16()
            : null;

        Filter[]? filters = null;

        if (flags.HasFlag(PlaceObjectFlags.HasFilterList))
        {
            var numFilters = reader.ReadUInt8();

            filters = new Filter[numFilters];

            for (var i = 0; i < numFilters; i++)
                filters[i] = Filter.Decode(ref reader);
        }

        BlendMode? blendMode = flags.HasFlag(PlaceObjectFlags.HasBlendMode)
            ? (BlendMode)reader.ReadUInt8()
            : null;

        bool? isBitmapCached = flags.HasFlag(PlaceObjectFlags.HasCacheAsBitmap)
            ? reader.ReadBoolean()
            : null;

        bool? isVisible = flags.HasFlag(PlaceObjectFlags.HasVisible)
            ? reader.ReadBoolean()
            : null;

        Color? backgroundColor = flags.HasFlag(PlaceObjectFlags.OpaqueBackground)
            ? Color.DecodeRgba(ref reader)
            : null;

        var clipActions = flags.HasFlag(PlaceObjectFlags.HasClipActions)
            ? ClipAction.DecodeCollection(ref reader, swfVersion)
            : null;

        return new PlaceObject3Tag(metadata, action, depth, className, matrix, colorTransform, flags, ratio, name, clipDepth, filters, blendMode, isBitmapCached, isVisible, backgroundColor, clipActions);
    }
}
