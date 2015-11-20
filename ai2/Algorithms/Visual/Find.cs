using System;

using Misc;

namespace Algorithms.Visual
{
    class Find
    {
        delegate bool delegateCompare(float oldValue, float compareValue);

        /** \brief finds minimum in a map around a radius
         * 
         * uses naive slow search
         * 
         */
        public static Vector2<int> findMinimum(Vector2<int> position, Map2d<float> image, uint radius)
        {
            return findLambdaUtilisation(position, image, radius, (old, compare) => compare < old);
        }

        public static Vector2<int> findMaximum(Vector2<int> position, Map2d<float> image, uint radius)
        {
            return findLambdaUtilisation(position, image, radius, (old, compare) => compare > old);
        }

        //
        //     Zzzzz
        //     zYyyz
        //     zyXyz
        //     zyyyz
        //     zzzzz
        //
        public static Vector2<int> findNearestPositionWhereMapIs(bool value, Vector2<int> position, Map2d<bool> image, uint radius, out bool found)
        {
            Vector2<int> outwardIteratorOffsetUnbound;
            Vector2<int> borderMin;
            Vector2<int> borderMax;
            Vector2<int> one;
            Vector2<int> positionAsInt;

            found = false;

            outwardIteratorOffsetUnbound = new Vector2<int>();
            outwardIteratorOffsetUnbound.x = 0;
            outwardIteratorOffsetUnbound.y = 0;

            borderMin = new Vector2<int>();
            borderMin.x = 0;
            borderMin.y = 0;

            borderMax = new Vector2<int>();
            borderMax.x = (int)image.getWidth();
            borderMax.y = (int)image.getLength();

            positionAsInt = new Vector2<int>();
            positionAsInt.x = (int)position.x;
            positionAsInt.y = (int)position.y;

            one = new Vector2<int>();
            one.x = 1;
            one.y = 1;

            for(;;)
            {
                Vector2<int> iteratorOffsetBoundMin;
                Vector2<int> iteratorOffsetBoundMax;
                int x, y;
                
                if (-outwardIteratorOffsetUnbound.x > radius)
                {
                    break;
                }



                iteratorOffsetBoundMin = Vector2<int>.max(borderMin, outwardIteratorOffsetUnbound + positionAsInt, outwardIteratorOffsetUnbound + positionAsInt, outwardIteratorOffsetUnbound + positionAsInt);
                iteratorOffsetBoundMax = Vector2<int>.min(borderMax, outwardIteratorOffsetUnbound.getScaled(-1) + one + positionAsInt, borderMax, borderMax);

                for (y = (int)(iteratorOffsetBoundMin.y); y < iteratorOffsetBoundMax.y; y++ )
                {
                    for( x = (int)(iteratorOffsetBoundMin.x); x < iteratorOffsetBoundMax.x; x++ )
                    {
                        // just find at the border
                        if (y == (int)(iteratorOffsetBoundMin.y) || y == iteratorOffsetBoundMax.y - 1 || x == (int)(iteratorOffsetBoundMin.x) || x == iteratorOffsetBoundMax.x - 1)
                        {
                            bool valueAtPoint;

                            valueAtPoint = image.readAt(x, y);

                            if (valueAtPoint == value)
                            {
                                found = true;

                                Vector2<int> result;

                                result = new Vector2<int>();
                                result.x = x;
                                result.y = y;

                                return result;
                            }
                        }
                    }
                }

                outwardIteratorOffsetUnbound.x--;
                outwardIteratorOffsetUnbound.y--;
            }

            found = false;

            return new Vector2<int>();            
        }

        private static Vector2<int> findLambdaUtilisation(Vector2<int> position, Map2d<float> image, uint radius, delegateCompare compare)
        {
            Vector2<int> borderMin;
            Vector2<int> borderMax;
            Vector2<int> min;
            Vector2<int> max;
            Vector2<int> rangeMin;
            Vector2<int> rangeMax;
            int x, y;
            float minimum;
            int minimumX, minimumY;

            Vector2<int> resultPosition;
            
            borderMin = new Vector2<int>();
            borderMin.x = 0;
            borderMin.y = 0;

            borderMax = new Vector2<int>();
            borderMax.x = (int)image.getWidth();
            borderMax.y = (int)image.getLength();

            rangeMin = new Vector2<int>();
            rangeMin.x = position.x - (int)radius;
            rangeMin.y = position.y - (int)radius;

            rangeMax = new Vector2<int>();
            rangeMax.x = position.x + (int)radius;
            rangeMax.y = position.y + (int)radius;

            // small hack because there is no overload for 
            min = Vector2<int>.max(rangeMin, borderMin, borderMin, borderMin);
            max = Vector2<int>.min(rangeMax, borderMax, borderMax, borderMax);

            minimum = image.readAt(position.x, position.y);
            minimumX = position.x;
            minimumY = position.y;

            for( y = min.y; y < max.y; y++ )
            {
                for( x = min.x; x < max.x; x++ )
                {
                    float sampled;

                    sampled = image.readAt(x, y);

                    if( compare(minimum, sampled) )
                    {
                        minimumX = x;
                        minimumY = y;
                    }
                }
            }

            resultPosition = new Vector2<int>();
            resultPosition.x = minimumX;
            resultPosition.y = minimumY;

            return resultPosition;
        }

        // TODO< find maximum >
    }
}
