using ShockwaveFlash.Exceptions;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.DisplayList;

namespace ShockwaveFlash.Tags.DisplayList;

public sealed class PlaceObject2Tag : Tag
{
    public PlaceObjectAction Action { get; set; }

    public ushort Depth { get; set; }

    public Matrix? Matrix { get; set; }

    public ColorTransform? ColorTransform { get; set; }

    public PlaceObjectFlags Flags { get; set; }

    public ushort? Ratio { get; set; }

    public string? Name { get; set; }

    public ushort? ClipDepth { get; set; }

    public IReadOnlyList<ClipAction>? ClipActions { get; set; }

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

    public PlaceObject2Tag(TagMetadata metadata, PlaceObjectAction action, ushort depth, Matrix? matrix, ColorTransform? colorTransform, PlaceObjectFlags flags, ushort? ratio, string? name, ushort? clipDepth, IReadOnlyList<ClipAction>? clipActions) : base(metadata)
    {
        Action = action;
        Depth = depth;
        Matrix = matrix;
        ColorTransform = colorTransform;
        Flags = flags;
        Ratio = ratio;
        Name = name;
        ClipDepth = clipDepth;
        ClipActions = clipActions;
    }

    public static PlaceObject2Tag Decode(MemoryReader reader, TagMetadata metadata, byte swfVersion)
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
            _ => throw new SwfFormatException("PlaceObject2 has neither a move nor a character; the action combination is invalid.")
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

        var clipActions = flags.HasFlag(PlaceObjectFlags.HasClipActions)
            ? ClipAction.DecodeCollection(reader, swfVersion)
            : null;

        return new PlaceObject2Tag(metadata, action, depth, matrix, colorTransform, flags, ratio, name, clipDepth, clipActions);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt8((byte)Flags);
        writer.WriteUInt16(Depth);

        if (HasCharacter)
        {
            var id = Action switch
            {
                PlaceObjectAction.PlaceObjectActionPlace place => place.Id,
                PlaceObjectAction.PlaceObjectActionReplace replace => replace.Id,
                _ => throw new NotSupportedException("Invalid PlaceObject2Tag action combination.")
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

        if (HasClipActions && ClipActions is { } clipActions)
            ClipAction.EncodeCollection(writer, clipActions, swfVersion);
    }
}
