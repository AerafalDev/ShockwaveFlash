// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types;
using ShockwaveFlash.Types.DisplayList;

namespace ShockwaveFlash.Tags.DisplayList;

public sealed record PlaceObject2Tag(
    TagMetadata Metadata,
    PlaceObjectAction Action,
    ushort Depth,
    Matrix? Matrix,
    ColorTransform? ColorTransform,
    PlaceObjectFlags Flags,
    ushort? Ratio,
    string? Name,
    ushort? ClipDepth,
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

    public static PlaceObject2Tag Decode(ref SpanReader reader, TagMetadata metadata, byte swfVersion)
    {
        var flags = (PlaceObjectFlags)reader.ReadUInt8();
        var depth = reader.ReadUInt16();

        var hasMove = flags.HasFlag(PlaceObjectFlags.Move);
        var hasid = flags.HasFlag(PlaceObjectFlags.HasCharacter);

        var action = (hasMove, hasid) switch
        {
            (true, false) => PlaceObjectAction.Modify(),
            (false, true) => PlaceObjectAction.Place(reader.ReadUInt16()),
            (true, true) => PlaceObjectAction.Replace(reader.ReadUInt16()),
            _ => throw new NotSupportedException("Invalid PlaceObject2Tag action combination.")
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

        var clipActions = flags.HasFlag(PlaceObjectFlags.HasClipActions)
            ? ClipAction.DecodeCollection(ref reader, swfVersion)
            : null;

        return new PlaceObject2Tag(metadata, action, depth, matrix, colorTransform, flags, ratio, name, clipDepth, clipActions);
    }
}
