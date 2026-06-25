// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types;
using ShockwaveFlash.Types.DisplayList;
using ShockwaveFlash.Types.Filter;

namespace ShockwaveFlash.Tags.DisplayList;

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

    public static PlaceObject3Tag Decode(MemoryReader reader, TagMetadata metadata, byte swfVersion)
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
            ? Types.Matrix.Decode(reader)
            : null;

        ColorTransform? colorTransform = flags.HasFlag(PlaceObjectFlags.HasColorTransform)
            ? Types.ColorTransform.DecodeRgba(reader)
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
                filters[i] = Filter.Decode(reader);
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
            ? Color.DecodeRgba(reader)
            : null;

        var clipActions = flags.HasFlag(PlaceObjectFlags.HasClipActions)
            ? ClipAction.DecodeCollection(reader, swfVersion)
            : null;

        return new PlaceObject3Tag(metadata, action, depth, className, matrix, colorTransform, flags, ratio, name, clipDepth, filters, blendMode, isBitmapCached, isVisible, backgroundColor, clipActions);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16((ushort)Flags);
        writer.WriteUInt16(Depth);

        var hasClassName = HasClassName || (HasImage && !HasCharacter);

        if (hasClassName && ClassName is { } className)
            writer.WriteNullTerminatedString(className);

        if (HasCharacter)
        {
            var id = Action switch
            {
                PlaceObjectAction.PlaceObjectActionPlace place => place.Id,
                PlaceObjectAction.PlaceObjectActionReplace replace => replace.Id,
                _ => throw new NotSupportedException("Invalid PlaceObject3Tag action combination.")
            };

            writer.WriteUInt16(id);
        }

        if (HasMatrix && Matrix is { } matrix)
            matrix.Encode(writer);

        if (HasColorTransform && ColorTransform is { } colorTransform)
            colorTransform.EncodeRgba(writer);

        if (HasRatio && Ratio is { } ratio)
            writer.WriteUInt16(ratio);

        if (HasName && Name is { } name)
            writer.WriteNullTerminatedString(name);

        if (HasClipDepth && ClipDepth is { } clipDepth)
            writer.WriteUInt16(clipDepth);

        if (HasFilterList && Filters is { } filters)
        {
            writer.WriteUInt8((byte)filters.Length);

            for (var i = 0; i < filters.Length; i++)
                filters[i].Encode(writer);
        }

        if (HasBlendMode && BlendMode is { } blendMode)
            writer.WriteUInt8((byte)blendMode);

        if (HasCacheAsBitmap && IsBitmapCached is { } isBitmapCached)
            writer.WriteBoolean(isBitmapCached);

        if (HasVisible && IsVisible is { } isVisible)
            writer.WriteBoolean(isVisible);

        if (OpaqueBackground && BackgroundColor is { } backgroundColor)
            backgroundColor.EncodeRgba(writer);

        if (HasClipActions && ClipActions is { } clipActions)
            ClipAction.EncodeCollection(writer, clipActions, swfVersion);
    }
}
