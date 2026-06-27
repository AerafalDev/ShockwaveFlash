using ShockwaveFlash.Exceptions;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.DisplayList;
using ShockwaveFlash.Types.Filter;

namespace ShockwaveFlash.Tags.DisplayList;

public sealed class PlaceObject3Tag : Tag
{
    private const PlaceObjectFlags PreservedFlags =
        PlaceObjectFlags.HasImage | PlaceObjectFlags.HasClassName | (PlaceObjectFlags)0x8000;

    public PlaceObjectAction Action { get; set; }

    public ushort Depth { get; set; }

    public string? ClassName { get; set; }

    public Matrix? Matrix { get; set; }

    public ColorTransform? ColorTransform { get; set; }

    public PlaceObjectFlags Flags { get; set; }

    public ushort? Ratio { get; set; }

    public string? Name { get; set; }

    public ushort? ClipDepth { get; set; }

    public Filter[]? Filters { get; set; }

    public BlendMode? BlendMode { get; set; }

    public bool? IsBitmapCached { get; set; }

    public bool? IsVisible { get; set; }

    public Color? BackgroundColor { get; set; }

    public IReadOnlyList<ClipAction>? ClipActions { get; set; }

    public bool Move =>
        Action is PlaceObjectAction.PlaceObjectActionModify or PlaceObjectAction.PlaceObjectActionReplace;

    public bool HasCharacter =>
        Action is PlaceObjectAction.PlaceObjectActionPlace or PlaceObjectAction.PlaceObjectActionReplace;

    public bool HasMatrix =>
        Matrix is not null;

    public bool HasColorTransform =>
        ColorTransform is not null;

    public bool HasRatio =>
        Ratio is not null;

    public bool HasName =>
        Name is not null;

    public bool HasClipDepth =>
        ClipDepth is not null;

    public bool HasClipActions =>
        ClipActions is not null;

    public bool HasFilterList =>
        Filters is not null;

    public bool HasBlendMode =>
        BlendMode is not null;

    public bool HasCacheAsBitmap =>
        IsBitmapCached is not null;

    public bool HasClassName =>
        Flags.HasFlag(PlaceObjectFlags.HasClassName);

    public bool HasImage =>
        Flags.HasFlag(PlaceObjectFlags.HasImage);

    public bool HasVisible =>
        IsVisible is not null;

    public bool OpaqueBackground =>
        BackgroundColor is not null;

    public PlaceObject3Tag(TagMetadata metadata, PlaceObjectAction action, ushort depth, string? className, Matrix? matrix, ColorTransform? colorTransform, PlaceObjectFlags flags, ushort? ratio, string? name, ushort? clipDepth, Filter[]? filters, BlendMode? blendMode, bool? isBitmapCached, bool? isVisible, Color? backgroundColor, IReadOnlyList<ClipAction>? clipActions) : base(metadata)
    {
        Action = action;
        Depth = depth;
        ClassName = className;
        Matrix = matrix;
        ColorTransform = colorTransform;
        Flags = flags;
        Ratio = ratio;
        Name = name;
        ClipDepth = clipDepth;
        Filters = filters;
        BlendMode = blendMode;
        IsBitmapCached = isBitmapCached;
        IsVisible = isVisible;
        BackgroundColor = backgroundColor;
        ClipActions = clipActions;
    }

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
            _ => throw new SwfFormatException("PlaceObject3 has neither a move nor a character; the action combination is invalid.")
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
        var flags = ComputeFlags();

        writer.WriteUInt16((ushort)flags);
        writer.WriteUInt16(Depth);

        var hasClassName = flags.HasFlag(PlaceObjectFlags.HasClassName) || (flags.HasFlag(PlaceObjectFlags.HasImage) && !flags.HasFlag(PlaceObjectFlags.HasCharacter));

        if (hasClassName && ClassName is { } className)
            writer.WriteNullTerminatedString(className);

        if (HasCharacter)
        {
            var id = Action switch
            {
                PlaceObjectAction.PlaceObjectActionPlace place => place.Id,
                PlaceObjectAction.PlaceObjectActionReplace replace => replace.Id,
                _ => throw new SwfFormatException("Invalid PlaceObject3Tag action combination.")
            };

            writer.WriteUInt16(id);
        }

        if (Matrix is { } matrix)
            matrix.Encode(writer);

        if (ColorTransform is { } colorTransform)
            colorTransform.EncodeRgba(writer);

        if (Ratio is { } ratio)
            writer.WriteUInt16(ratio);

        if (Name is { } name)
            writer.WriteNullTerminatedString(name);

        if (ClipDepth is { } clipDepth)
            writer.WriteUInt16(clipDepth);

        if (Filters is { } filters)
        {
            writer.WriteUInt8((byte)filters.Length);

            for (var i = 0; i < filters.Length; i++)
                filters[i].Encode(writer);
        }

        if (BlendMode is { } blendMode)
            writer.WriteUInt8((byte)blendMode);

        if (IsBitmapCached is { } isBitmapCached)
            writer.WriteBoolean(isBitmapCached);

        if (IsVisible is { } isVisible)
            writer.WriteBoolean(isVisible);

        if (BackgroundColor is { } backgroundColor)
            backgroundColor.EncodeRgba(writer);

        if (ClipActions is { } clipActions)
            ClipAction.EncodeCollection(writer, clipActions, swfVersion);
    }

    private PlaceObjectFlags ComputeFlags()
    {
        var flags = Flags & PreservedFlags;

        if (Move)
            flags |= PlaceObjectFlags.Move;

        if (HasCharacter)
            flags |= PlaceObjectFlags.HasCharacter;

        if (HasMatrix)
            flags |= PlaceObjectFlags.HasMatrix;

        if (HasColorTransform)
            flags |= PlaceObjectFlags.HasColorTransform;

        if (HasRatio)
            flags |= PlaceObjectFlags.HasRatio;

        if (HasName)
            flags |= PlaceObjectFlags.HasName;

        if (HasClipDepth)
            flags |= PlaceObjectFlags.HasClipDepth;

        if (HasClipActions)
            flags |= PlaceObjectFlags.HasClipActions;

        if (HasFilterList)
            flags |= PlaceObjectFlags.HasFilterList;

        if (HasBlendMode)
            flags |= PlaceObjectFlags.HasBlendMode;

        if (HasCacheAsBitmap)
            flags |= PlaceObjectFlags.HasCacheAsBitmap;

        if (HasVisible)
            flags |= PlaceObjectFlags.HasVisible;

        if (OpaqueBackground)
            flags |= PlaceObjectFlags.OpaqueBackground;

        return flags;
    }
}
