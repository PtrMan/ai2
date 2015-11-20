using System;

using Misc;

namespace ComputationBackend.cs
{
    class OperatorColorTransformToHsl
    {
        public void calculate(ComputationBackend.OpenCl.ComputeContext computeContext)
        {
            int x, y;

            for( y = 0; y < inputRgb.getLength(); y++ )
            {
                for( x = 0; x < inputRgb.getWidth(); x++ )
                {
                    ColorHsl pixelHsl;
                    ColorRgb pixelRgb;

                    pixelRgb = inputRgb.readAt(x, y);

                    pixelHsl = ColorConversion.rgbToHsl(pixelRgb);

                    resultMap.writeAt(x, y, pixelHsl);
                }
            }
        }

        public Map2d<ColorHsl> resultMap;
        public Map2d<ColorRgb> inputRgb;
    }
}
