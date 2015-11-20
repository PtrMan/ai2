using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misc
{
    class RandomUtil
    {
        // this is the generator for the Halton sequence numbers

        static public float radicalInverse(int n, int baseValue)
        {
            float Val = 0.0f;
            float InvBase = 1.0f / (float)baseValue;
            float InvBi = InvBase;

            while (n > 0)
            {
                int Di = (n % baseValue);
                Val += ((float)Di * InvBi);
                n = (int)((float)n * InvBase);
                InvBi *= InvBase;
            }

            return Val;
        }

    }
}
