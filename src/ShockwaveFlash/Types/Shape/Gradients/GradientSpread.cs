namespace ShockwaveFlash.Types.Shape.Gradients;

public enum GradientSpread : byte
{
    Pad = 0,
    Reflect = 1,
    Repeat = 2
}

public static class GradientSpreadExtensions
{
    extension(GradientSpread)
    {
        public static GradientSpread Parse(byte bits)
        {
            if (bits is 3)
                bits = 0;

            return (GradientSpread)bits;
        }
    }
}
