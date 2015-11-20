using System;
using System.Collections.Generic;


namespace Algorithms.Visual
{
    class RadialKernelDetector
    {
        public void configure(int kernelSize)
        {
            int ix;
            int iy;
            float normalisation;
            
            this.kernelSize = kernelSize;
            kernelRadius = kernelSize / 2;

            this.kernel = new float[kernelSize * kernelSize];

            float variance = (float)System.Math.Sqrt(0.15f);
            normalisation = Misc.Gaussian.calculateGaussianDistribution(0.0f, 0.0f, variance);

            for( iy = 0; iy < kernelSize; iy++ )
            {
                for( ix = 0; ix < kernelSize; ix++ )
                {
                    float relativeX;
                    float relativeY;
                    float distanceFromCenter;
                    float gaussianResult;
                    float normalizedGaussianResult;

                    relativeX = ((float)ix / kernelSize) * 2.0f - 1.0f;
                    relativeY = ((float)ix / kernelSize) * 2.0f - 1.0f;

                    distanceFromCenter = (float)System.Math.Sqrt(relativeX*relativeX + relativeY*relativeY);

                    gaussianResult = Misc.Gaussian.calculateGaussianDistribution(distanceFromCenter, 0.0f, variance);
                    normalizedGaussianResult = gaussianResult / normalisation;

                    kernel[ix + iy * kernelSize] = normalizedGaussianResult;
                }
            }
        }

        public void calculateKernelsForPositions(Map2d<float> map, ref Misc.Vector2<int>[] kernelPositions, ref float[] kernelValues )
        {
            int i;

            for( i = 0; i < kernelPositions.Length; i++ )
            {
                kernelValues[i] = multiplyKernelWithMapAtCenter(kernelPositions[i].x, kernelPositions[i].y, map);
            }
        }

        private float multiplyKernelWithMapAtCenter(int centerX, int centerY, Map2d<float> map)
        {
            int ix;
            int iy;
            int relativeCenterX;
            int relativeCenterY;
            float result;

            result = 0.0f;

            if(
                centerX < kernelRadius || centerX + kernelRadius >= map.getWidth() ||
                centerY < kernelRadius || centerY + kernelRadius >= map.getLength()
            )
            {
                return 0.0f;
            }

            relativeCenterX = centerX - kernelRadius;
            relativeCenterY = centerY - kernelRadius;

            for( iy = 0; iy < kernelSize; iy++ )
            {
                for( ix = 0; ix < kernelSize; ix++ )
                {
                    float readValue;

                    readValue = map.readAt(ix + relativeCenterX, iy + relativeCenterY);

                    result += (readValue * kernel[ix + iy*kernelSize]);
                }
            }

            return result;
        }

        private int kernelRadius; // kernelSize / 2
        private int kernelSize;
        private float[] kernel;

    }
}
