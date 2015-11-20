using System;
using System.Collections.Generic;

namespace ComputationBackend.cs
{
    class OperatorGaborFilter
    {
        public void calculate(ComputationBackend.OpenCl.ComputeContext computeContext)
        {
            outputMap = Misc.Convolution2d.convolution(inputMap, gaborKernel);
        }

        /**
         * 
         * must be called before calculate is called
         * 
         */
        public void calculateKernel()
        {
            gaborKernel = Math.GaborKernel.generateGaborKernel(kernelWidth, kernelPhi, kernelLamda, kernelPhaseOffset, kernelSpartialRatioAspect);
        }

        // settings for the kernel
        public int kernelWidth;
        public float kernelPhi;
        public float kernelLamda;
        public float kernelPhaseOffset = 0.0f;
        public float kernelSpartialRatioAspect = 1.0f;

        public Map2d<float> inputMap;
        public Map2d<float> outputMap;

        public Map2d<float> gaborKernel;
    }
}
