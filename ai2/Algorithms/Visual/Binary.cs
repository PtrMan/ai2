using System;

namespace Algorithms.Visual
{
    class Binary
    {
        public static Map2d<bool> negate(Map2d<bool> input)
        {
            Map2d<bool> result;
            int x, y;

            result = new Map2d<bool>(input.getWidth(), input.getLength());

            for (y = 0; y < input.getLength(); y++)
            {
                for (x = 0; x < input.getWidth(); x++)
                {
                    
                    result.writeAt(x, y, !input.readAt(x, y));
                }
            }

            return result;
        }


        public static Map2d<bool> corode(Map2d<bool> input)
        {
            Map2d<bool> result;
            int x, y;

            result = new Map2d<bool>(input.getWidth(), input.getLength());

            for( y = 1; y < input.getLength() - 1; y++ )
            {
                for( x = 1; x < input.getWidth() - 1; x++ )
                {
                    // optimized

                    if(
                        !input.readAt(x-1,y-1) ||
                        !input.readAt(x,y-1) ||
                        !input.readAt(x+1,y-1) ||
                        
                        !input.readAt(x-1,y) ||
                        !input.readAt(x,y) ||
                        !input.readAt(x+1,y) ||
                        
                        !input.readAt(x-1,y+1) ||
                        !input.readAt(x,y+1) ||
                        !input.readAt(x+1,y+1)
                    )
                    {
                        continue;
                    }

                    result.writeAt(x, y, true);
                }
            }

            return result;
        }

        // edge thinning
        // http://fourier.eng.hmc.edu/e161/lectures/morphology/node2.html
        public static Map2d<bool> edgeThinning(Map2d<bool> input)
        {
            Map2d<bool> result;
            int x, y;
            bool[] neightbors;

            neightbors = new bool[8];

            result = new Map2d<bool>(input.getWidth(), input.getLength());

            for( y = 1; y < input.getLength() - 1; y++ )
            {
                for( x = 1; x < input.getWidth() - 1; x++ )
                {
                    int zeroCrossing;
                    int setPixels;
                    int i;
                    bool deletePixel;

                    neightbors[0] = input.readAt(x-1,y-1);
                    neightbors[1] = input.readAt(x,y-1);
                    neightbors[2] = input.readAt(x+1,y-1);
                        
                    neightbors[3] = input.readAt(x+1,y);
                    neightbors[4] = input.readAt(x+1,y+1);

                    neightbors[5] = input.readAt(x,y+1);
                    neightbors[6] = input.readAt(x-1,y+1);

                    neightbors[7] = input.readAt(x-1,y);

                    // count zero crossing
                    zeroCrossing = 0;

                    for( i = 0; i < 8-1; i++ )
                    {
                        if( neightbors[i] && !neightbors[i+1] )
                        {
                            zeroCrossing++;
                        }
                    }

                    // count set pixels
                    setPixels = 0;

                    for( i = 0; i < 8; i++ )
                    {
                        if( neightbors[i] )
                        {
                            setPixels++;
                        }
                    }
                    
                    if(
                        setPixels == 0 ||
                        setPixels == 1 ||
                        setPixels == 7 || 
                        setPixels == 8 || 
                        zeroCrossing >= 2
                    )
                    {
                        deletePixel = true;
                    }
                    else
                    {
                        deletePixel = false;
                    }

                    if( !deletePixel )
                    {
                        result.writeAt(x, y, true);
                    }
                }
            }

            return result;
        }

        // http://fourier.eng.hmc.edu/e161/lectures/morphology/node3.html
        public static Map2d<bool> skeletalize(Map2d<bool> input)
        {
            Map2d<int> counterMap;
            int x, y;
            int k;
            int[] minArray;
            int[] maxArray;
            Map2d<bool> output;

            minArray = new int[5];
            maxArray = new int[5];

            counterMap = new Map2d<int>(input.getWidth(), input.getLength());
            output = new Map2d<bool>(input.getWidth(), input.getLength());

            for( y = 0; y < input.getLength(); y++ )
            {
                for( x = 0; x < input.getWidth(); x++ )
                {
                    if( input.readAt(x, y) )
                    {
                        counterMap.writeAt(x, y, 1);
                    }
                }
            }

            k = 0;

            for(;;)
            {
                bool changeMade;

                k += 1;

                changeMade = false;

                for( y = 1; y < input.getLength() - 1; y++ )
                {
                    for( x = 1; x < input.getWidth() - 1; x++ )
                    {
                        int min;
                        int newValue;

                        if( counterMap.readAt(x, y) != k )
                        {
                            continue;
                        }
                        // we are here if it is the case

                        changeMade = true;

                        //minArray[0] = counterMap.readAt(x-1, y-1);
                        minArray[0] = counterMap.readAt(x, y-1);
                        //minArray[2] = counterMap.readAt(x+1, y-1);

                        minArray[1] = counterMap.readAt(x-1, y);
                        minArray[2] = counterMap.readAt(x, y);
                        //minArray[2] = counterMap.readAt(x, y);
                        minArray[3] = counterMap.readAt(x+1, y);

                        //minArray[6] = counterMap.readAt(x-1, y+1);
                        minArray[4] = counterMap.readAt(x, y+1);
                        //minArray[8] = counterMap.readAt(x+1, y+1);

                        min = minOfArray(ref minArray);
                        newValue = min + 1;

                        counterMap.writeAt(x, y, newValue);

                        //output.writeAt(x, y, true);
                    }
                }

                if( !changeMade )
                {
                    break;
                }
            }

            for (y = 1; y < input.getLength()-1; y++)
            {
                for (x = 1; x < input.getWidth()-1; x++)
                {
                    int compare;

                    compare = counterMap.readAt(x, y);

                    if( compare == 0 )
                    {
                        continue;
                    }

                    maxArray[0] = counterMap.readAt(x+1, y);
                    maxArray[1] = counterMap.readAt(x-1, y);
                    maxArray[2] = counterMap.readAt(x, y+1);
                    maxArray[3] = counterMap.readAt(x, y-1);
                    maxArray[4] = counterMap.readAt(x, y);

                    if( compare == maxOfArray(ref maxArray) )
                    {
                        output.writeAt(x, y, true);
                    }
                }
            }


            return output;
        }

        public static Map2d<bool> threshold(Map2d<float> input, float threshold)
        {
            Map2d<bool> result;
            int x, y;

            result = new Map2d<bool>(input.getWidth(), input.getLength());

            for( y = 0; y < input.getLength(); y++ )
            {
                for( x = 0; x < input.getWidth(); x++ )
                {
                    if( input.readAt(x, y) > threshold )
                    {
                        result.writeAt(x, y, true);
                    }
                }
            }

            return result;
        }

        private static int minOfArray(ref int[] array)
        {
            int min;
            int i;

            min = array[0];

            for( i = 1; i < array.Length; i++ )
            {
                if( array[i] < min )
                {
                    min = array[i];
                }
            }

            return min;
        }

        private static int maxOfArray(ref int[] array)
        {
            int max;
            int i;

            max = array[0];

            for (i = 1; i < array.Length; i++)
            {
                if (array[i] > max)
                {
                    max = array[i];
                }
            }

            return max;
        }
    }
}
