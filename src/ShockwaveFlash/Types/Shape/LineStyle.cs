// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace ShockwaveFlash.Types.Shape;

public sealed class LineStyle
{
    public int Width { get; set; }

    public FillStyle FillStyle { get; set; }

    public LineStyleFlags Flags { get; set; }

    public float MiterLimit { get; set; }

    public bool AllowClose =>
        !Flags.HasFlag(LineStyleFlags.NoClose);

    public bool AllowScaleX =>
        !Flags.HasFlag(LineStyleFlags.NoHScale);

    public bool AllowScaleY =>
        !Flags.HasFlag(LineStyleFlags.NoVScale);

    public bool PixelHinted =>
        Flags.HasFlag(LineStyleFlags.PixelHinting);

    public LineCapStyle StartCap =>
        (LineCapStyle)((byte)(Flags & LineStyleFlags.StartCapStyle) >> 6);

    public LineCapStyle EndCap =>
        (LineCapStyle)((byte)(Flags & LineStyleFlags.EndCapStyle) >> 8);

    public LineJoinStyle JoinStyle =>
        (Flags & LineStyleFlags.JoinStyle) switch
        {
            LineStyleFlags.Round => new LineJoinStyleRound(),
            LineStyleFlags.Bevel => new LineJoinStyleBevel(),
            LineStyleFlags.Miter => new LineJoinStyleMiter(MiterLimit),
            _ => throw new NotSupportedException($"Line join style {Flags & LineStyleFlags.JoinStyle} is not supported.")
        };

    public LineStyle()
    {
        FillStyle = new FillStyleSolid(Color.Black);
    }

    public LineStyle(int width, FillStyle fillStyle, LineStyleFlags flags, float miterLimit)
    {
        Width = width;
        FillStyle = fillStyle;
        Flags = flags;
        MiterLimit = miterLimit;
    }

    public LineStyle WithAllowClose(bool allowClose)
    {
        Flags = Flags.Set(LineStyleFlags.NoClose, !allowClose);
        return this;
    }

    public LineStyle WithAllowScaleX(bool allowScaleX)
    {
        Flags = Flags.Set(LineStyleFlags.NoHScale, !allowScaleX);
        return this;
    }

    public LineStyle WithAllowScaleY(bool allowScaleY)
    {
        Flags = Flags.Set(LineStyleFlags.NoVScale, !allowScaleY);
        return this;
    }

    public LineStyle WithIsPixelHinted(bool pixelHinted)
    {
        Flags = Flags.Set(LineStyleFlags.PixelHinting, pixelHinted);
        return this;
    }

    public LineStyle WithStartCap(LineCapStyle style)
    {
        Flags = Flags.Remove(LineStyleFlags.StartCapStyle);
        Flags = Flags.Add((LineStyleFlags)((ushort)style << 6));
        return this;
    }

    public LineStyle WithEndCap(LineCapStyle style)
    {
        Flags = Flags.Remove(LineStyleFlags.EndCapStyle);
        Flags = Flags.Add((LineStyleFlags)((ushort)style << 8));
        return this;
    }

    public LineStyle WithJoinStyle(LineJoinStyle style)
    {
        Flags = Flags.Remove(LineStyleFlags.JoinStyle);

        switch (style)
        {
            case LineJoinStyleBevel:
                Flags = Flags.Add(LineStyleFlags.Bevel);
                break;

            case LineJoinStyleRound:
                Flags = Flags.Add(LineStyleFlags.Round);
                break;

            case LineJoinStyleMiter miter:
                Flags = Flags.Add(LineStyleFlags.Miter);
                MiterLimit = miter.MiterLimit;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(style));
        }

        return this;
    }

    public LineStyle WithFillStyle(FillStyle fillStyle)
    {
        Flags = Flags.Set(LineStyleFlags.HasFill, fillStyle is not FillStyleSolid);
        FillStyle = fillStyle;
        return this;
    }

    public LineStyle WithColor(Color color)
    {
        Flags = Flags.Remove(LineStyleFlags.HasFill);
        FillStyle = new FillStyleSolid(color);
        return this;
    }

    public LineStyle WithWidth(int width)
    {
        Width = width;
        return this;
    }

    public static LineStyle Decode(ref SpanReader reader, byte swfVersion, byte shapeVersion)
    {
        var width = reader.ReadUInt16();

        if (shapeVersion < 4)
        {
            var color = shapeVersion >= 3 ? Color.DecodeRgba(ref reader) : Color.DecodeRgb(ref reader);

            return new LineStyle()
                .WithWidth(width)
                .WithColor(color);
        }

        var flags = (LineStyleFlags)reader.ReadUInt16();

        if (flags.HasFlag(LineStyleFlags.JoinStyle))
        {
            Debug.Fail("Invalid line join style.");
            flags = flags.Remove(LineStyleFlags.JoinStyle);
        }

        if (flags.HasFlag(LineStyleFlags.StartCapStyle))
        {
            Debug.Fail("Invalid line start cap style.");
            flags = flags.Remove(LineStyleFlags.StartCapStyle);
        }

        if (flags.HasFlag(LineStyleFlags.EndCapStyle))
        {
            Debug.Fail("Invalid line end cap style.");
            flags = flags.Remove(LineStyleFlags.EndCapStyle);
        }

        var miterLimit = (flags & LineStyleFlags.JoinStyle) is LineStyleFlags.Miter
            ? reader.ReadFixed8()
            : 0f;

        var fillStyle = flags.HasFlag(LineStyleFlags.HasFill)
            ? FillStyle.Decode(ref reader, swfVersion, shapeVersion)
            : new FillStyleSolid(Color.DecodeRgba(ref reader));

        return new LineStyle(width, fillStyle, flags, miterLimit);
    }

    public static (LineStyle, LineStyle) DecodeMorph(ref SpanReader reader, byte shapeVersion)
    {
        var startWidth = reader.ReadUInt16();
        var endWidth = reader.ReadUInt16();

        if (shapeVersion < 2)
        {
            var startColor = Color.DecodeRgba(ref reader);
            var endColor = Color.DecodeRgba(ref reader);

            var startLineStyle = new LineStyle()
                .WithWidth(startWidth)
                .WithColor(startColor);

            var endLineStyle = new LineStyle()
                .WithWidth(endWidth)
                .WithColor(endColor);

            return (startLineStyle, endLineStyle);
        }

        var flags = (LineStyleFlags)reader.ReadUInt16();

        if (flags.HasFlag(LineStyleFlags.JoinStyle))
        {
            Debug.Fail("Invalid line join style.");
            flags = flags.Remove(LineStyleFlags.JoinStyle);
        }

        if (flags.HasFlag(LineStyleFlags.StartCapStyle))
        {
            Debug.Fail("Invalid line start cap style.");
            flags = flags.Remove(LineStyleFlags.StartCapStyle);
        }

        if (flags.HasFlag(LineStyleFlags.EndCapStyle))
        {
            Debug.Fail("Invalid line end cap style.");
            flags = flags.Remove(LineStyleFlags.EndCapStyle);
        }

        var miterLimit = (flags & LineStyleFlags.JoinStyle) is LineStyleFlags.Miter
            ? reader.ReadFixed8()
            : 0f;

        var (startFillStyle, endFillStyle) = flags.HasFlag(LineStyleFlags.HasFill)
            ? FillStyle.DecodeMorph(ref reader)
            : (new FillStyleSolid(Color.DecodeRgba(ref reader)), new FillStyleSolid(Color.DecodeRgba(ref reader)));

        return (new LineStyle(startWidth, startFillStyle, flags, miterLimit), new LineStyle(endWidth, endFillStyle, flags, miterLimit));
    }
}
