using System;

using Math;

namespace Misc
{
    class Convolution2d
    {
        public static Map2d<float> convolution(Map2d<float> input, Map2d<float> inputKernel)
        {
            float[,] inputAsMatrix, kernelAsMatrix;
            Map2d<float> resultMap;
            int x, y;

            inputAsMatrix = convertMapToArray(input);
            kernelAsMatrix = convertMapToArray(inputKernel);

            resultMap = new Map2d<float>(input.getWidth(), input.getLength());

            for( y = 0; y < input.getLength() - inputKernel.getLength(); y++ )
            {
                for( x = 0; x < input.getWidth() - inputKernel.getWidth(); x++ )
                {
                    float value;

                    value = convolutionAt(ref inputAsMatrix, ref kernelAsMatrix, x, y);
                    resultMap.writeAt(x, y, value);
                }
            }

            return resultMap;

            // old broken not working code

            /*
                combinedWidth = calculateCombinedWidthPowerTwo((int)input.getWidth(), (int)inputKernel.getWidth());
            combinedHeight = calculateCombinedWidthPowerTwo((int)input.getLength(), (int)inputKernel.getLength());

            paddedInput = getPadded(input, combinedWidth, combinedHeight);
            paddedKernel = getPadded(inputKernel, combinedWidth, combinedHeight);

            //return cutAndConvertToMap(paddedKernel, 0, 0, paddedInput.GetLength(0), paddedInput.GetLength(1));

            fft.FFT fftKernel = new fft.FFT(paddedKernel);
            fftKernel.doFft(fft.FFT.EnumDirection.FORWARD);
            fft.ComplexNumber[,] fftOfKernel = fftKernel.Output;

            fft.FFT fftInput = new fft.FFT(paddedInput);
            fftInput.doFft(fft.FFT.EnumDirection.FORWARD);
            fft.ComplexNumber[,] fftOfInput = fftInput.Output;

            //fft.ComplexNumber[,] multipliedFft = multiplyComplexNumberArray(ref fftOfKernel, ref fftOfInput);
            fft.ComplexNumber[,] multipliedFft = NativeMatrix.multiply<fft.ComplexNumber>(fftOfKernel, fftOfInput);

            fft.FFT fftInverse = new fft.FFT(multipliedFft);
            fftInverse.doFft(fft.FFT.EnumDirection.BACKWARD);
            double[,] inverseResult = fftInverse.inverseResult;

            // grab the result and output it
            return cutAndConvertToMap(inverseResult, (int)inputKernel.getWidth() / 2, (int)inputKernel.getLength() / 2, (int)input.getWidth(), (int)input.getLength());
             */

        }

        private static float convolutionAt(ref float[,] input, ref float[,] kernel, int startX, int startY)
        {
            float result;
            int x, y;

            result = 0.0f;

            for( y = 0; y < kernel.GetLength(1); y++ )
            {
                for( x = 0; x < kernel.GetLength(0); x++ )
                {
                    result += (input[x + startX, y + startY] * kernel[x, y]);
                }
            }

            return result;
        }

        private static float[,] convertMapToArray(Map2d<float> map)
        {
            float[,] result;
            int x, y;

            result = new float[map.getWidth(), map.getLength()];

            for( y = 0; y < map.getLength(); y++ )
            {
                for( x = 0; x < map.getWidth(); x++ )
                {
                    result[x, y] = map.readAt(x, y);
                }
            }

            return result;
        }

        private static Map2d<float> cutAndConvertToMap(double[,] values, int startX, int startY, int width, int height)
        {
            int x, y;
            Map2d<float> result;

            result = new Map2d<float>((uint)width, (uint)height);

            for( y = 0; y < height; y++ )
            {
                for( x = 0; x < width; x++ )
                {
                    float value;

                    value = (float)values[x + startX, y + startY];
                    result.writeAt(x, y, value);
                }
            }

            return result;
        }

        private static double[,] getPadded(Map2d<float> map, int width, int height)
        {
            double[,] resultArray;
            int x, y;

            int mapWidth, mapHeight;

            mapWidth = (int)map.getWidth();
            mapHeight = (int)map.getLength();

            resultArray = new double[width, height];

            for( y = 0; y < mapHeight; y++ )
            {
                for( x = 0; x < mapWidth; x++ )
                {
                    resultArray[ x,  y] = map.readAt(x, y);
                }
            }

            return resultArray;
        }


        private static fft.ComplexNumber[,] multiplyComplexNumberArray(ref fft.ComplexNumber[,] a, ref fft.ComplexNumber[,] b)
        {
            int x, y;
            fft.ComplexNumber[,] result;

            System.Diagnostics.Debug.Assert(a.GetLength(0) == b.GetLength(0));
            System.Diagnostics.Debug.Assert(a.GetLength(1) == b.GetLength(1));

            result = new fft.ComplexNumber[a.GetLength(0), a.GetLength(1)];

            for( y = 0; y < a.GetLength(1); y++ )
            {
                for( x = 0; x < a.GetLength(0); x++ )
                {
                    result[x, y] = a[x, y] * b[x, y];
                }
            }

            return result;
        }

        private static int calculateCombinedWidthPowerTwo(int a, int b)
        {
            int temp;
            int factor;

            temp = a + b;

            factor = 1;

            for(;;)
            {
                if (factor >=temp)
                {
                    return factor;
                }

                // else we are here

                factor *= 2;
            }
        }
    }
}
