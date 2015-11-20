using System;
using System.Reflection;
using System.IO;

using OpenCL.Net;

namespace ComputationBackend.OpenCl
{
    class OperatorSkeletalize
    {
        public void initialize(ComputeContext computeContext, Misc.Vector2<int> inputMapSize)
        {
            ErrorCode errorCode;
            

            bufferForCounterMap = Cl.CreateBuffer<int>(computeContext.context, MemFlags.AllocHostPtr, (int)(inputMapSize.x * inputMapSize.y), out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            bufferForCounterOutputMap = Cl.CreateBuffer<int>(computeContext.context, MemFlags.AllocHostPtr, (int)(inputMapSize.x * inputMapSize.y), out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            bufferForChangeMade = Cl.CreateBuffer<int>(computeContext.context, MemFlags.AllocHostPtr, 1, out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");


            program = Cl.CreateProgramWithSource(computeContext.context, 1, new[] { getOpenClSource() }, null, out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.BuildProgram(program, 1, new[] { computeContext.chosenDevice }, "", null, IntPtr.Zero);
            if (errorCode != ErrorCode.Success)
            {
                OpenCL.Net.InfoBuffer logInfoBuffer = Cl.GetProgramBuildInfo(program, computeContext.chosenDevice, ProgramBuildInfo.Log, out errorCode);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

                throw new ComputeContext.OpenClError();
            }


            kernelNarrow = Cl.CreateKernel(program, "narrow", out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.SetKernelArg(kernelNarrow, 4, (IntPtr)4, inputMapSize.x);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg(kernelNarrow, 5, (IntPtr)4, inputMapSize.y);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
        }

        public void calculate(ComputeContext computeContext, ResourceMetric resourceMetric)
        {
            ErrorCode errorCode;

            OpenCL.Net.Event eventWriteBufferForCounterMap;
            OpenCL.Net.Event eventReadBufferForChangeMade;
            OpenCL.Net.Event eventReadBufferForCounterMapOutput;

            OpenCL.Net.IMem<int> bufferForCurrentCounterMapInput;
            OpenCL.Net.IMem<int> bufferForCurrentCounterMapOutput;

            int[] counterMapForOpenCl;
            int[] changeMadeForOpenCl;
            int i;
            int k;

            resourceMetric.startTimer("visual", "skeletalize", "initInputMap");

            counterMapForOpenCl = new int[inputMap.getWidth() * inputMap.getLength()];

            for (i = 0; i < counterMapForOpenCl.Length; i++ )
            {
                if( inputMap.unsafeGetValues()[i] )
                {
                    counterMapForOpenCl[i] = 1;
                }
            }

            resourceMetric.stopTimer();

            errorCode = Cl.EnqueueWriteBuffer<int>(computeContext.commandQueue, bufferForCounterMap, OpenCL.Net.Bool.False, counterMapForOpenCl, 0, new Event[] { }, out eventWriteBufferForCounterMap);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.Flush(computeContext.commandQueue);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.WaitForEvents(1, new Event[] { eventWriteBufferForCounterMap });
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            Cl.ReleaseEvent(eventWriteBufferForCounterMap);

            bufferForCurrentCounterMapInput = bufferForCounterMap;
            bufferForCurrentCounterMapOutput = bufferForCounterOutputMap;

            resourceMetric.startTimer("visual", "skeletalize", "main k loop");

            for( k = 1;; k++ )
            {
                OpenCL.Net.Event eventWriteBufferForChangeMade;
                OpenCL.Net.Event eventExecutedNarrow;

                OpenCL.Net.IMem<int> swapingBuffer;

                changeMadeForOpenCl = new int[1];

                errorCode = Cl.EnqueueWriteBuffer<int>(computeContext.commandQueue, bufferForChangeMade, OpenCL.Net.Bool.False, changeMadeForOpenCl, 0, new Event[] { }, out eventWriteBufferForChangeMade);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

                errorCode = Cl.SetKernelArg<int>(kernelNarrow, 0, bufferForCurrentCounterMapInput);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

                errorCode = Cl.SetKernelArg<int>(kernelNarrow, 1, bufferForCurrentCounterMapOutput);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

                errorCode = Cl.SetKernelArg<int>(kernelNarrow, 2, bufferForChangeMade);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

                // set k
                errorCode = Cl.SetKernelArg(kernelNarrow, 3, (IntPtr)4, k);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

                IntPtr[] globalWorkSize = new IntPtr[] { (IntPtr)(inputMap.getWidth() / 2), (IntPtr)(inputMap.getLength()) };
                IntPtr[] localWorkSize = new IntPtr[] { (IntPtr)32, (IntPtr)4 };

                errorCode = Cl.EnqueueNDRangeKernel(computeContext.commandQueue, kernelNarrow, 2, null, globalWorkSize, localWorkSize, 1, new Event[] { eventWriteBufferForChangeMade }, out eventExecutedNarrow);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");


                errorCode = Cl.Flush(computeContext.commandQueue);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
                errorCode = Cl.WaitForEvents(1, new Event[] { eventExecutedNarrow });
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

                // read back change made
                errorCode = Cl.EnqueueReadBuffer(computeContext.commandQueue, bufferForChangeMade, OpenCL.Net.Bool.False, changeMadeForOpenCl, 0, new Event[] { }, out eventReadBufferForChangeMade);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

                errorCode = Cl.Flush(computeContext.commandQueue);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
                errorCode = Cl.WaitForEvents(1, new Event[] { eventReadBufferForChangeMade });
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

                Cl.ReleaseEvent(eventWriteBufferForChangeMade);
                Cl.ReleaseEvent(eventReadBufferForChangeMade);

                if( changeMadeForOpenCl[0] == 0 )
                {
                    break;
                }

                // swap
                swapingBuffer = bufferForCurrentCounterMapInput;
                bufferForCurrentCounterMapInput = bufferForCurrentCounterMapOutput;
                bufferForCurrentCounterMapOutput = swapingBuffer;

            }

            resourceMetric.stopTimer();

            
            
            Map2d<int> counterMap;

            counterMap = new Map2d<int>(inputMap.getWidth(), inputMap.getLength());

            errorCode = Cl.EnqueueReadBuffer(computeContext.commandQueue, bufferForCurrentCounterMapOutput, OpenCL.Net.Bool.False, counterMap.unsafeGetValues(), 0, new Event[] { }, out eventReadBufferForCounterMapOutput);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.Flush(computeContext.commandQueue);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.WaitForEvents(1, new Event[] { eventReadBufferForCounterMapOutput });
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            int x, y;
            int[] maxArray;

            maxArray = new int[5];

            resourceMetric.startTimer("visual", "skeletalize", "transfer result");

            int[] counterMapArray = counterMap.unsafeGetValues();
            bool[] resultMapArray = resultMap.unsafeGetValues();

            int mapWidth = (int)inputMap.getWidth();

            for (y = 1; y < inputMap.getLength() - 1; y++)
            {
                for (x = 1; x < inputMap.getWidth() - 1; x++)
                {
                    int compare;

                    compare = counterMapArray[(x) + (y) * mapWidth];

                    if (compare == 0)
                    {
                        resultMapArray[x + y * mapWidth] = false;

                        continue;
                    }

                    maxArray[0] = counterMapArray[(x + 1) + (y) * mapWidth];
                    maxArray[1] = counterMapArray[(x - 1) + (y) * mapWidth];
                    maxArray[2] = counterMapArray[(x) + (y + 1) * mapWidth];
                    maxArray[3] = counterMapArray[(x) + (y - 1) * mapWidth];
                    maxArray[4] = counterMapArray[(x) + (y) * mapWidth];

                    if (compare == maxOfArray(ref maxArray))
                    {
                        resultMapArray[x + y*mapWidth] = true;
                    }
                    else
                    {
                        resultMapArray[x + y * mapWidth] = false;
                    }
                }
            }

            resourceMetric.stopTimer();
        }

        public void dispose()
        {
            Cl.ReleaseMemObject(bufferForCounterMap);
            Cl.ReleaseMemObject(bufferForCounterOutputMap);
            Cl.ReleaseMemObject(bufferForChangeMade);

            Cl.ReleaseProgram(program);

            Cl.ReleaseKernel(kernelNarrow);
        }

        private string getOpenClSource()
        {
            string programLocation = Assembly.GetEntryAssembly().Location;

            string pathToLoad = Path.Combine(Path.GetDirectoryName(programLocation), "..\\..\\", "ComputationBackend\\OpenCl\\src\\Skeletalize.cl");

            string openClSource = File.ReadAllText(pathToLoad);

            return openClSource;
        }

        // still needed, maybe not needed if it is moved into openCl
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

        public Map2d<bool> inputMap;
        public Map2d<bool> resultMap;

        private OpenCL.Net.IMem<int> bufferForCounterMap;
        private OpenCL.Net.IMem<int> bufferForCounterOutputMap;
        private OpenCL.Net.IMem<int> bufferForChangeMade;



        private OpenCL.Net.Program program;

        private OpenCL.Net.Kernel kernelNarrow;
    }
}
