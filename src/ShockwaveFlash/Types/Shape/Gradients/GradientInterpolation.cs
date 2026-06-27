namespace ShockwaveFlash.Types.Shape.Gradients;

public enum GradientInterpolation
{
    Rgb = 0,
    LinearRgb = 1
}

public static class GradientInterpolationExtensions
{
    extension(GradientInterpolation)
    {
        public static GradientInterpolation Parse(byte bits)
        {
            if (bits is 2 or 3)
                bits = 0;

            return (GradientInterpolation)bits;
        }
    }
}
