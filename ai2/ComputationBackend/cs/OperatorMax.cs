using System;
using System.Collections.Generic;

namespace ComputationBackend.cs
{
    class OperatorMax
    {
        public void calculate(ComputationBackend.OpenCl.ComputeContext computeContext)
        {
            int x, y;

            System.Diagnostics.Debug.Assert(inputA.getWidth() == inputB.getWidth());
            System.Diagnostics.Debug.Assert(inputA.getLength() == inputB.getLength());

            result = new Map2d<float>((uint)inputA.getWidth(), (uint)inputA.getLength());

            for( y = 0; y < inputA.getLength(); y++ )
            {
                for( x = 0; x < inputA.getWidth(); x++ )
                {
                    float valueA, valueB, valueResult;

                    valueA = inputA.readAt(x, y);
                    valueB = inputB.readAt(x, y);

                    valueResult = System.Math.Max(valueA, valueB);

                    result.writeAt(x, y, valueResult);
                }
            }
        }

        public Map2d<float> inputA;
        public Map2d<float> inputB;

        public Map2d<float> result;
    }
}
