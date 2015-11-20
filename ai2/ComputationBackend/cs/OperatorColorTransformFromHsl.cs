using System;

using Misc;

namespace ComputationBackend.cs
{
    class OperatorColorTransformFromHsl
    {
        public void calculate(ComputationBackend.OpenCl.ComputeContext computeContext)
        {
            int x, y;

            for (y = 0; y < inputHsl.getLength(); y++)
            {
                for (x = 0; x < inputHsl.getWidth(); x++)
                {
                    ColorHsl pixelHsl;
                    ColorRgb pixelRgb;

                    pixelHsl = inputHsl.readAt(x, y);

                    pixelRgb = ColorConversion.hslToRgb(pixelHsl);

                    resultMap.writeAt(x, y, pixelRgb);
                }
            }
        }

        public Map2d<ColorRgb> resultMap;
        public Map2d<ColorHsl> inputHsl;
    }
}
