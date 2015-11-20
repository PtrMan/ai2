using System;
using System.Collections.Generic;

using System.Reflection;
using System.IO;

using OpenCL.Net;


namespace ComputationBackend.OpenCl
{
    class OperatorBlur
    {
        public void initialize(ComputeContext computeContext, int kernelRadius, Misc.Vector2<int> inputMapSize)
        {
            ErrorCode errorCode;
            float[] kernelArray;

            OpenCL.Net.Event eventWriteBufferCompletedKernel;

            bufferForInputMap = Cl.CreateBuffer<float>(computeContext.context, MemFlags.AllocHostPtr, (int)(inputMapSize.x * inputMapSize.y), out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            bufferForTemporary = Cl.CreateBuffer<float>(computeContext.context, MemFlags.AllocHostPtr, (int)(inputMapSize.x * inputMapSize.y), out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            bufferForOutputMap = Cl.CreateBuffer<float>(computeContext.context, MemFlags.AllocHostPtr, (int)(inputMapSize.x * inputMapSize.y), out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            kernelArray = calculateKernel(kernelRadius);

            bufferForKernelArray = Cl.CreateBuffer<float>(computeContext.context, MemFlags.AllocHostPtr, kernelArray.Length, out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            // copy kernel into buffer
            errorCode = Cl.EnqueueWriteBuffer<float>(computeContext.commandQueue, bufferForKernelArray, OpenCL.Net.Bool.True, kernelArray, 0, null, out eventWriteBufferCompletedKernel);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");



            string programLocation = Assembly.GetEntryAssembly().Location;

            string pathToLoad = Path.Combine(Path.GetDirectoryName(programLocation), "..\\..\\", "ComputationBackend\\OpenCl\\src\\Blur.cl");

            string openClSource = File.ReadAllText(pathToLoad);


            program = Cl.CreateProgramWithSource(computeContext.context, 1, new[] { openClSource }, null, out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.BuildProgram(program, 1, new[] { computeContext.chosenDevice }, "", null, IntPtr.Zero);
            if (errorCode != ErrorCode.Success)
            {
                OpenCL.Net.InfoBuffer logInfoBuffer = Cl.GetProgramBuildInfo(program, computeContext.chosenDevice, ProgramBuildInfo.Log, out errorCode);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

                throw new ComputeContext.OpenClError();
            }

            kernelBlurX = Cl.CreateKernel(program, "blurX", out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            //errorCode = Cl.SetKernelArg<float>(kernelBlurX, 0, bufferForInputMap);
            //ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg<float>(kernelBlurX, 1, bufferForKernelArray);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            //errorCode = Cl.SetKernelArg<float>(kernelBlurX, 2, bufferForTemporary);
            //ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg(kernelBlurX, 3, (IntPtr)4, kernelRadius);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg(kernelBlurX, 4, (IntPtr)4, inputMapSize.x);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");


            kernelBlurY = Cl.CreateKernel(program, "blurY", out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            //errorCode = Cl.SetKernelArg<float>(kernelBlurY, 0, bufferForTemporary);
            //ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg<float>(kernelBlurY, 1, bufferForKernelArray);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            //errorCode = Cl.SetKernelArg<float>(kernelBlurY, 2, bufferForOutputMap);
            //ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg(kernelBlurY, 3, (IntPtr)4, kernelRadius);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg(kernelBlurY, 4, (IntPtr)4, inputMapSize.x);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg(kernelBlurY, 5, (IntPtr)4, inputMapSize.y);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            Cl.ReleaseEvent(eventWriteBufferCompletedKernel);
        }

        public void calculate(ComputeContext computeContext)
        {
            ErrorCode errorCode;

            OpenCL.Net.Event eventWriteBufferForInputMap;
            OpenCL.Net.Event eventReadBufferForOutputMap;
            OpenCL.Net.Event eventExecutedKernelBlurX;
            OpenCL.Net.Event eventExecutedKernelBlurY;


            errorCode = Cl.EnqueueWriteBuffer<float>(computeContext.commandQueue, bufferForInputMap, OpenCL.Net.Bool.False, inputMap.unsafeGetValues(), 0, new Event[] { }, out eventWriteBufferForInputMap);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");


            IntPtr[] globalWorkSize = new IntPtr[] { (IntPtr)(inputMap.getWidth()/2), (IntPtr)(inputMap.getLength()) };
            IntPtr[] localWorkSize = new IntPtr[] { (IntPtr)32, (IntPtr)4 };

            // execute X
            errorCode = Cl.SetKernelArg<float>(kernelBlurX, 0, bufferForInputMap);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.SetKernelArg<float>(kernelBlurX, 2, bufferForTemporary);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.EnqueueNDRangeKernel(computeContext.commandQueue, kernelBlurX, 2, null, globalWorkSize, localWorkSize, 1, new Event[] { eventWriteBufferForInputMap }, out eventExecutedKernelBlurX);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.SetKernelArg<float>(kernelBlurY, 0, bufferForTemporary);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.SetKernelArg<float>(kernelBlurY, 2, bufferForOutputMap);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            // execute Y
            errorCode = Cl.EnqueueNDRangeKernel(computeContext.commandQueue, kernelBlurY, 2, null, globalWorkSize, localWorkSize, 1, new Event[] { eventExecutedKernelBlurX }, out eventExecutedKernelBlurY);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");


            // read back blured map
            errorCode = Cl.EnqueueReadBuffer(computeContext.commandQueue, bufferForOutputMap, OpenCL.Net.Bool.True, outputMap.unsafeGetValues(), 3, new Event[] { eventExecutedKernelBlurY, eventExecutedKernelBlurX, eventWriteBufferForInputMap }, out eventReadBufferForOutputMap);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            //Cl.Flush(computeContext.commandQueue);
            //Cl.WaitForEvents(1, new[] { eventReadBufferForOutputMap });

            // HACK
            System.Diagnostics.Debug.Assert(!float.IsNaN(outputMap.unsafeGetValues()[6]) && outputMap.unsafeGetValues()[6] < 50000.0f);
        }

        private static float[] calculateKernel(int radius)
        {
            float[] kernel;
            int di;

            // create kernel
            kernel = new float[(radius - 1) * 2 + 1];

            float variance;
            float normalisation;

            variance = (float)System.Math.Sqrt(0.15f);

            kernel[radius - 1] = 1.0f;

            normalisation = Misc.Gaussian.calculateGaussianDistribution(0.0f, 0.0f, variance);

            for (di = 1; di < radius; di++)
            {
                float gaussianResult;
                float normalizedResult;

                gaussianResult = Misc.Gaussian.calculateGaussianDistribution((float)(di) / (float)(radius), 0.0f, variance);
                normalizedResult = gaussianResult / normalisation;

                kernel[radius - 1 + di] = normalizedResult;
                kernel[radius - 1 - di] = normalizedResult;
            }

            normalizeKernelArray(ref kernel);

            return kernel;
        }

        private static void normalizeKernelArray(ref float[] kernel)
        {
            float sum;
            int i;

            sum = 0.0f;

            for( i = 0; i < kernel.Length; i++ )
            {
                sum += kernel[i];
            }

            for (i = 0; i < kernel.Length; i++)
            {
                kernel[i] /= sum;
            }
        }

        public Map2d<float> inputMap;
        public Map2d<float> outputMap;

        private OpenCL.Net.Program program;

        private OpenCL.Net.Kernel kernelBlurX;
        private OpenCL.Net.Kernel kernelBlurY;

        private OpenCL.Net.IMem<float> bufferForInputMap;
        private OpenCL.Net.IMem<float> bufferForTemporary;
        private OpenCL.Net.IMem<float> bufferForOutputMap;

        private OpenCL.Net.IMem<float> bufferForKernelArray;
    }
}
