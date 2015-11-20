using System;
using System.Collections.Generic;

namespace NeuralNetworks.AdaptiveResonanceTheory
{
    class Test0
    {
        public static void test0()
        {
            AdaptiveResonanceTheory2 art2;
            AdaptiveResonanceTheory2.in_param param;

            List<Math.DynamicVector<float>> samples;

            samples = new List<Math.DynamicVector<float>>();
            samples.Add(new Math.DynamicVector<float>(3));
            samples[0][0] = 0.0f;
            samples[0][1] = 0.5f;
            samples[0][2] = 0.0f;

            param = new AdaptiveResonanceTheory2.in_param();
            param.beta = 0.5f; // default
            param.vigilance = 0.1f; // default
            param.pass = 50;
            param.alpha = 0.5f;

            AdaptiveResonanceTheory2.Clust clust;

            clust = new AdaptiveResonanceTheory2.Clust();

            art2 = new AdaptiveResonanceTheory2();
            art2.art2A(samples, param, clust);
        }
    }
}
