using System;
using System.Collections.Generic;

using Misc;

namespace ComputationBackend.cs
{
    class OperatorColorTransform
    {
        public void calculate(ComputationBackend.OpenCl.ComputeContext computeContext)
        {
            int x, y;

            for( y = 0; y < inputRgb.getLength(); y++ )
            {
                for( x = 0; x < inputRgb.getWidth(); x++ )
                {
                    ColorRgb readColor;
                    float one;
                    float zero;
                    float value;

                    readColor = inputRgb.readAt(x, y);

                    one = (readColor * colorForOne).getMagnitude();
                    zero = (readColor * colorForZero).getMagnitude();

                    if( (one + zero) < 0.000001f )
                    {
                        value = 1.0f;
                    }
                    else
                    {
                        value = one / (one + zero);
                    }
                    
                    resultMap.writeAt(x, y, value);
                }
            }
        }

        public ColorRgb colorForZero;
        public ColorRgb colorForOne;

        public Map2d<float> resultMap;
        public Map2d<ColorRgb> inputRgb;
    }
}
