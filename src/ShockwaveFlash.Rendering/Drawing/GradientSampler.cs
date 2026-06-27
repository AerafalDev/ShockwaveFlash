using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Drawing;

public static class GradientSampler
{
    public static Color Sample(IReadOnlyList<GradientStop> stops, float t)
    {
        var lo = stops[0];
        var hi = stops[^1];

        for (var i = 0; i < stops.Count - 1; i++)
        {
            var a = stops[i].Ratio / 255f;
            var b = stops[i + 1].Ratio / 255f;

            if (t <= a)
                return stops[i].Color;

            if (t < b)
            {
                lo = stops[i];
                hi = stops[i + 1];
                break;
            }
        }

        if (t >= stops[^1].Ratio / 255f)
            return stops[^1].Color;

        var span = (hi.Ratio - lo.Ratio) / 255f;
        var k = span <= 0 ? 0f : (t - (lo.Ratio / 255f)) / span;

        return new Color(
            LerpLinear(lo.Color.R, hi.Color.R, k),
            LerpLinear(lo.Color.G, hi.Color.G, k),
            LerpLinear(lo.Color.B, hi.Color.B, k),
            (byte)Math.Clamp((int)Math.Round(lo.Color.A + ((hi.Color.A - lo.Color.A) * k)), 0, 255));
    }

    private static byte LerpLinear(byte a, byte b, float k)
    {
        var linear = SrgbToLinear(a / 255f) + ((SrgbToLinear(b / 255f) - SrgbToLinear(a / 255f)) * k);
        return (byte)Math.Clamp((int)Math.Round(LinearToSrgb(linear) * 255f), 0, 255);
    }

    private static float SrgbToLinear(float c)
    {
        return c <= 0.04045f ? c / 12.92f : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);
    }

    private static float LinearToSrgb(float c)
    {
        return c <= 0.0031308f ? c * 12.92f : (1.055f * MathF.Pow(c, 1f / 2.4f)) - 0.055f;
    }
}
