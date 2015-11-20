using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms
{
    class MapAlgorithms
    {
        public static Map2d<bool> mapOr3(Map2d<bool> a, Map2d<bool> b, Map2d<bool> c)
        {
            return mapOr2(mapOr2(a, b), c);
        }

        public static Map2d<bool> mapOr2(Map2d<bool> a, Map2d<bool> b)
        {
            // TODO< assert same size
            Map2d<bool> resultMap;
            int x, y;

            resultMap = new Map2d<bool>(a.getWidth(), a.getLength());

            for( y = 0; y < a.getLength(); y++ )
            {
                for( x = 0; x < a.getWidth(); x++ )
                {
                    bool bit;

                    bit = a.readAt(x, y) || b.readAt(x, y);

                    resultMap.writeAt(x, y, bit);
                }
            }

            return resultMap;
        }
    }
}
