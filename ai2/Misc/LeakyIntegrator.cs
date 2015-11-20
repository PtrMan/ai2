using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misc
{
    class LeakyIntegrator
    {
        public float value;
        public float leakFactor;

        public void step(float input, float timeDelta)
        {
            value = value * System.Math.Max(1.0f - timeDelta * leakFactor, 0.0f) + input * timeDelta;
        }
    }
}
