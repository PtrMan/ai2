using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms
{
    /** \brief calculates the average of a map and stores it into another (output) map
     * 
     */
    class BoxAverageFilter
    {
        public static Map2d<float> forFloat(Map2d<float> input, uint squareSize)
        {
            Map2d<float> resultMap;
            int x, y;

            resultMap = new Map2d<float>(input.getWidth() / squareSize, input.getLength() / squareSize);

            for( y = 0; y < resultMap.getLength(); y++ )
            {
                for( x = 0; x < resultMap.getWidth(); x++ )
                {
                    float averageResult;

                    averageResult = averageForFloat(input, x, y, (int)squareSize);

                    resultMap.writeAt(x, y, averageResult);
                }
            }

            return resultMap;
        }

        private static float averageForFloat(Map2d<float> input, int x, int y, int squareSize)
        {
            int ix, iy;
            float average;

            average = 0.0f;

            for( iy = y*squareSize; iy < y*(squareSize+1); iy++ )
            {
                for( ix = x*squareSize; ix < x*(squareSize+1); ix++ )
                {
                    average += input.readAt(ix, iy);
                }
            }

            return average/(float)(squareSize*squareSize);
        }
    }
}
