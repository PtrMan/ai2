using System;
using System.Collections.Generic;

using OpenCL.Net;

namespace ComputationBackend.OpenCl
{
    /**
     * assumptions
     * * length of positions/results are multiple of 32
     * 
     * 
     */
    class OperatorRadialKernel
    {
        public void initialize(ComputeContext computeContext, int kernelPositionsLength, Misc.Vector2<int> inputMapSize)
        {
            ErrorCode errorCode;
            
            OpenCL.Net.Event eventWriteBufferCompletedKernel;

            this.kernelPositionsLength = kernelPositionsLength;

            kernelResults = new float[kernelPositionsLength];

            bufferForPositions = Cl.CreateBuffer<int>(computeContext.context, MemFlags.AllocHostPtr, 2 * kernelPositionsLength, out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            bufferForInputMap = Cl.CreateBuffer<float>(computeContext.context, MemFlags.AllocHostPtr, (int)(inputMapSize.x * inputMapSize.y), out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            OpenCL.Net.IMem<float> bufferForKernel = Cl.CreateBuffer<float>(computeContext.context, MemFlags.AllocHostPtr, (kernelWidth * kernelWidth), out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            bufferForKernelResults = Cl.CreateBuffer<float>(computeContext.context, MemFlags.AllocHostPtr, kernelPositionsLength, out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");



            errorCode = Cl.EnqueueWriteBuffer<float>(computeContext.commandQueue, bufferForKernel, OpenCL.Net.Bool.True, this.kernelArray, 0, null, out eventWriteBufferCompletedKernel);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            program = Cl.CreateProgramWithSource(computeContext.context, 1, new[] { getProgramSource(inputMapSize.x, kernelPositionsLength) }, null, out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.BuildProgram(program, 1, new[] { computeContext.chosenDevice }, "", null, IntPtr.Zero);
            if (errorCode != ErrorCode.Success)
            {
                OpenCL.Net.InfoBuffer logInfoBuffer = Cl.GetProgramBuildInfo(program, computeContext.chosenDevice, ProgramBuildInfo.Log, out errorCode);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

                throw new ComputeContext.OpenClError();
            }

            

            kernel = Cl.CreateKernel(program, "kernel0", out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.SetKernelArg<float>(kernel, 0, bufferForInputMap);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg<float>(kernel, 1, bufferForKernel);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg<int>(kernel, 2, bufferForPositions);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg<float>(kernel, 3, bufferForKernelResults);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            Cl.ReleaseEvent(eventWriteBufferCompletedKernel);
        }

        public void createKernel(int kernelSize)
        {
            int ix;
            int iy;
            float normalisation;
            int kernelRadius;

            kernelWidth = kernelSize;
            kernelRadius = kernelSize / 2;

            kernelArray = new float[kernelSize * kernelSize];

            float variance = (float)System.Math.Sqrt(0.15f);
            normalisation = Misc.Gaussian.calculateGaussianDistribution(0.0f, 0.0f, variance);

            for (iy = 0; iy < kernelSize; iy++)
            {
                for (ix = 0; ix < kernelSize; ix++)
                {
                    float relativeX;
                    float relativeY;
                    float distanceFromCenter;
                    float gaussianResult;
                    float normalizedGaussianResult;

                    relativeX = ((float)ix / kernelSize) * 2.0f - 1.0f;
                    relativeY = ((float)ix / kernelSize) * 2.0f - 1.0f;

                    distanceFromCenter = (float)System.Math.Sqrt(relativeX * relativeX + relativeY * relativeY);

                    gaussianResult = Misc.Gaussian.calculateGaussianDistribution(distanceFromCenter, 0.0f, variance);
                    normalizedGaussianResult = gaussianResult / normalisation;

                    kernelArray[ix + iy * kernelSize] = normalizedGaussianResult;
                }
            }
        }

        public void calculate(ComputeContext computeContext)
        {
            ErrorCode errorCode;

            OpenCL.Net.Event eventReadBufferForResults;
            OpenCL.Net.Event eventExecutedKernel;
            OpenCL.Net.Event eventWriteBufferForInputMap;
            OpenCL.Net.Event eventWriteBufferCompletedPositions;

            System.Diagnostics.Debug.Assert(kernelPositionsLength >= kernelPositions.Length);
            System.Diagnostics.Debug.Assert((kernelPositions.Length % 32) == 0, "kernelPositions.Length must be of length % 32 == 0");

            //float[] arrayForInputMap = new float[inputMap.getWidth() * inputMap.getLength()];
            //int x, y;

            /*
            for( y = 0; y < inputMap.getLength(); y++ )
            {
                for( x = 0; x < inputMap.getWidth(); x++ )
                {
                    arrayForInputMap[x + y * inputMap.getWidth()] = inputMap.readAt(x, y);
                }
            }
             */

            
            int[] positionsArray = new int[2 * kernelPositions.Length];

            int i;

            for (i = 0; i < kernelPositions.Length; i++)
            {
                if( kernelPositions[i] == null )
                {
                    continue;
                }

                positionsArray[i * 2 + 0] = kernelPositions[i].x;
                positionsArray[i * 2 + 1] = kernelPositions[i].y;
            }

            
            errorCode = Cl.EnqueueWriteBuffer<int>(computeContext.commandQueue, bufferForPositions, OpenCL.Net.Bool.False, positionsArray, 0, null, out eventWriteBufferCompletedPositions);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.EnqueueWriteBuffer<float>(computeContext.commandQueue, bufferForInputMap, OpenCL.Net.Bool.False, inputMap.unsafeGetValues(), 0, new Event[] { }, out eventWriteBufferForInputMap);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            


            IntPtr[] globalWorkSize = new IntPtr[] { (IntPtr)kernelPositions.Length };
            IntPtr[] localWorkSize = new IntPtr[] { (IntPtr)32 };

            errorCode = Cl.EnqueueNDRangeKernel(computeContext.commandQueue, kernel, 1, null, globalWorkSize, localWorkSize, 2, new Event[] { eventWriteBufferForInputMap, eventWriteBufferCompletedPositions }, out eventExecutedKernel);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");


            // important so it doesn't crash
            errorCode = Cl.Flush(computeContext.commandQueue);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.WaitForEvents(1, new Event[] { eventExecutedKernel });
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.EnqueueReadBuffer(computeContext.commandQueue, bufferForKernelResults, OpenCL.Net.Bool.False, kernelResults, 1, new[] { eventExecutedKernel }, out eventReadBufferForResults);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.Flush(computeContext.commandQueue);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.WaitForEvents(1, new[] { eventReadBufferForResults });
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            Cl.ReleaseEvent(eventReadBufferForResults);
            Cl.ReleaseEvent(eventExecutedKernel);
            Cl.ReleaseEvent(eventWriteBufferForInputMap);
            Cl.ReleaseEvent(eventWriteBufferCompletedPositions);
        }

        private string getProgramSource(int inputMapWidth, int kernelPositionsLength)
        {
            ProgramRepresentation.Program program;

            string innerOpenclProgram;
            string outerOpenclProgram;

            Dictionary<string, int> arrayWidths;

            program = ProgramRepresentation.Program.createProgramForParallelRadialKernel();

            arrayWidths = new Dictionary<string, int>();
            arrayWidths.Add("kernelArray", kernelWidth);
            arrayWidths.Add("resultMap", kernelPositionsLength);

            innerOpenclProgram = ProgramRepresentation.OpenClGenerator.generateSource(program, inputMapWidth, arrayWidths);

            outerOpenclProgram = "";
            outerOpenclProgram += "__kernel void kernel0(__global float* inputMap, __global const float* kernelArray, __global int* positions, __global float* resultMap){";
            outerOpenclProgram += "const int indexX = get_global_id(0);\n";
            outerOpenclProgram += innerOpenclProgram;
            outerOpenclProgram += "}";

            return outerOpenclProgram;

        }

        // must be a length of multiples of 16
        public Misc.Vector2<int>[] kernelPositions;
        public float[] kernelResults;

        private int kernelWidth;
        private float[] kernelArray;

        // must be initialized before configure
        public Map2d<float> inputMap;

        
        private OpenCL.Net.Program program;
        private OpenCL.Net.Kernel kernel;

        private OpenCL.Net.IMem<float> bufferForKernelResults;
        private OpenCL.Net.IMem<float> bufferForInputMap;
        private OpenCL.Net.IMem<int> bufferForPositions;

        // used for checking if allocate array is large enougth
        private int kernelPositionsLength;
    }
}
