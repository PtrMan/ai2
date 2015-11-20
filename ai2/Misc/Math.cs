using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misc
{
    class Math
    {
        public static int modi(int value, int mod)
        {
            if( value < 0 )
            {
                return (value + mod) % mod;
            }
            else
            {
                return value % mod;
            }
        }

        public static int powi(int value, int baseValue)
        {
            int result;
            int i;

            result = 1;

            for( i = 0; i < baseValue; i++ )
            {
                result *= 2;
            }

            return result;
        }
    }
}
