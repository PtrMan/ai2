using System;
using System.Collections.Generic;

using System.Reflection;
using System.IO;

using Misc;

using OpenCL.Net;

namespace ComputationBackend.OpenCl
{
    /**
     * operator which finds the nearest positions points where the map is true in a specific radius
     * is used for tracking the motion of borders
     * 
     */
    class OperatorFindNearestPosition
    {

        public void initialize(ComputeContext computeContext, int searchRadius, Misc.Vector2<int> inputMapSize)
        {
            List<Vector2<int>> relativePositions;
            ErrorCode errorCode;
            int[] relativePositionsArray;
            OpenCL.Net.Event eventWriteBufferForRelativePositions;

            relativePositions = calculateRelativePositionsForRadius(searchRadius);

            numberOfAllocatedInputAndOutputPositions = 50000;

            bufferForInputMap = Cl.CreateBuffer<int>(computeContext.context, MemFlags.AllocHostPtr, (int)(inputMapSize.x * inputMapSize.y), out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            bufferForRelativePositions = Cl.CreateBuffer<int>(computeContext.context, MemFlags.AllocHostPtr, relativePositions.Count * 2, out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            relativePositionsArray = convertRelativePositionsToArray(relativePositions);

            // copy relative positions into buffer
            errorCode = Cl.EnqueueWriteBuffer<int>(computeContext.commandQueue, bufferForRelativePositions, OpenCL.Net.Bool.True, relativePositionsArray, 0, new Event[] { }, out eventWriteBufferForRelativePositions);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            bufferForInputPositions = Cl.CreateBuffer<int>(computeContext.context, MemFlags.AllocHostPtr, numberOfAllocatedInputAndOutputPositions * 2, out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            bufferForOutputPositions = Cl.CreateBuffer<int>(computeContext.context, MemFlags.AllocHostPtr, numberOfAllocatedInputAndOutputPositions * 2, out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            bufferForFoundNewPosition = Cl.CreateBuffer<int>(computeContext.context, MemFlags.AllocHostPtr, numberOfAllocatedInputAndOutputPositions, out errorCode);
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


            kernelNearestPoint = Cl.CreateKernel(program, "findNearestPoint", out errorCode);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.SetKernelArg<int>(kernelNearestPoint, 0, bufferForInputMap);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg<int>(kernelNearestPoint, 1, bufferForRelativePositions);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg<int>(kernelNearestPoint, 2, bufferForInputPositions);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg<int>(kernelNearestPoint, 3, bufferForOutputPositions);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg<int>(kernelNearestPoint, 4, bufferForFoundNewPosition);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg(kernelNearestPoint, 5, (IntPtr)4, relativePositions.Count); // number of relative positions
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg(kernelNearestPoint, 6, (IntPtr)4, inputMapSize.x);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.SetKernelArg(kernelNearestPoint, 7, (IntPtr)4, inputMapSize.y);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            Cl.ReleaseEvent(eventWriteBufferForRelativePositions);
        }

        public void calculate(ComputeContext computeContext)
        {
            ErrorCode errorCode;
            int lengthOfPositionsForOpenCl;
            int numberOfPositionsForOpenCl;

            OpenCL.Net.Event eventWriteBufferInputPositions;
            OpenCL.Net.Event eventWriteBufferForInputMap;
            OpenCL.Net.Event eventExecutedKernelNearestPoint;
            OpenCL.Net.Event eventReadBufferForResultPositions;
            OpenCL.Net.Event eventReadBufferForFoundNewPosition;

            numberOfPositionsForOpenCl = inputPositions.Length;
            numberOfPositionsForOpenCl = numberOfPositionsForOpenCl + 32 + (32 - (numberOfPositionsForOpenCl % 32));

            lengthOfPositionsForOpenCl = numberOfPositionsForOpenCl * 2;

            if (lengthOfPositionsForOpenCl > numberOfAllocatedInputAndOutputPositions*2)
            {
                // we need to reallocate the buffer to the right size

                numberOfAllocatedInputAndOutputPositions = lengthOfPositionsForOpenCl;

                Cl.ReleaseMemObject(bufferForInputPositions);
                Cl.ReleaseMemObject(bufferForOutputPositions);
                Cl.ReleaseMemObject(bufferForFoundNewPosition);


                bufferForInputPositions = Cl.CreateBuffer<int>(computeContext.context, MemFlags.AllocHostPtr, numberOfAllocatedInputAndOutputPositions * 2, out errorCode);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

                bufferForOutputPositions = Cl.CreateBuffer<int>(computeContext.context, MemFlags.AllocHostPtr, numberOfAllocatedInputAndOutputPositions * 2, out errorCode);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

                bufferForFoundNewPosition = Cl.CreateBuffer<int>(computeContext.context, MemFlags.AllocHostPtr, numberOfAllocatedInputAndOutputPositions, out errorCode);
                ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            }

            // copy the positions

            int[] inputPositionsForOpenCl;

            inputPositionsForOpenCl = new int[lengthOfPositionsForOpenCl];

            int i;

            for (i = 0; i < inputPositions.Length; i++ )
            {
                inputPositionsForOpenCl[i * 2 + 0] = inputPositions[i].x;
                inputPositionsForOpenCl[i * 2 + 1] = inputPositions[i].y;
            }

            errorCode = Cl.EnqueueWriteBuffer<int>(computeContext.commandQueue, bufferForInputPositions, OpenCL.Net.Bool.False, inputPositionsForOpenCl, 0, new Event[] { }, out eventWriteBufferInputPositions);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            // copy input map

            int[] translatedInputMap;

            translatedInputMap = new int[inputMap.getWidth() * inputMap.getLength()];

            for (i = 0; i < inputMap.getWidth() * inputMap.getLength(); i++ )
            {
                if( inputMap.unsafeGetValues()[i] )
                {
                    translatedInputMap[i] = 1;
                }
            }

            errorCode = Cl.EnqueueWriteBuffer<int>(computeContext.commandQueue, bufferForInputMap, OpenCL.Net.Bool.False, translatedInputMap, 0, new Event[] { }, out eventWriteBufferForInputMap);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            //errorCode = Cl.SetKernelArg(kernelNearestPoint, 5, (IntPtr)4, /*lengthOfPositionsForOpenCl*/inputPositions.Length); // number of relative positions
            //ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            // call kernel

            IntPtr[] globalWorkSize = new IntPtr[] { (IntPtr)32, (IntPtr)(numberOfPositionsForOpenCl/32) };
            IntPtr[] localWorkSize = new IntPtr[] { (IntPtr)32, (IntPtr)1 };

            errorCode = Cl.EnqueueNDRangeKernel(computeContext.commandQueue, kernelNearestPoint, 2, null, globalWorkSize, localWorkSize, 2, new Event[] { eventWriteBufferInputPositions, eventWriteBufferForInputMap }, out eventExecutedKernelNearestPoint);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            int[] resultPositionsFromOpenCl;
            int[] resultfoundNewPositionsFromOpenCl;



            resultPositionsFromOpenCl = new int[lengthOfPositionsForOpenCl];
            resultfoundNewPositionsFromOpenCl = new int[lengthOfPositionsForOpenCl / 2];

            // read results back and copy into arrays
            errorCode = Cl.EnqueueReadBuffer(computeContext.commandQueue, bufferForOutputPositions, OpenCL.Net.Bool.False, resultPositionsFromOpenCl, 1, new[] { eventExecutedKernelNearestPoint }, out eventReadBufferForResultPositions);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.EnqueueReadBuffer(computeContext.commandQueue, bufferForFoundNewPosition, OpenCL.Net.Bool.False, resultfoundNewPositionsFromOpenCl, 1, new[] { eventExecutedKernelNearestPoint }, out eventReadBufferForFoundNewPosition);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            errorCode = Cl.Flush(computeContext.commandQueue);
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");
            errorCode = Cl.WaitForEvents(2, new Event[] { eventReadBufferForResultPositions, eventReadBufferForFoundNewPosition });
            ComputeContext.throwErrorIfNotSuccessfull(errorCode, "");

            // copy back
            outputPositions = new Vector2<int>[inputPositions.Length];
            foundNewPositions = new bool[inputPositions.Length];

            for( i = 0; i < inputPositions.Length; i++ )
            {
                // check output position
                // NOTE< works only if kernel is prepared >
                {
                    Vector2<int> inputPosition = inputPositions[i];

                    Vector2<int> outputPosition2;

                    outputPosition2 = new Vector2<int>();
                    outputPosition2.x = resultPositionsFromOpenCl[i * 2 + 0];
                    outputPosition2.y = resultPositionsFromOpenCl[i * 2 + 1];


                    

                    if (System.Math.Abs(outputPosition2.x - inputPosition.x) > 20 || System.Math.Abs(outputPosition2.y - inputPosition.y) > 20)
                    {
                        int here0 = 0;
                    }
                }


                Vector2<int> outputPosition;


                outputPosition = new Vector2<int>();
                outputPosition.x = resultPositionsFromOpenCl[i * 2 + 0];
                outputPosition.y = resultPositionsFromOpenCl[i * 2 + 1];


                outputPositions[i] = outputPosition;

                if( resultfoundNewPositionsFromOpenCl[i] == 1 )
                {
                    foundNewPositions[i] = true;
                }
            }

            Cl.ReleaseEvent(eventWriteBufferInputPositions);
            Cl.ReleaseEvent(eventWriteBufferForInputMap);
            Cl.ReleaseEvent(eventExecutedKernelNearestPoint);
            Cl.ReleaseEvent(eventReadBufferForResultPositions);
            Cl.ReleaseEvent(eventReadBufferForFoundNewPosition);
        }

        //
        //     Zzzzz
        //     zYyyz
        //     zyXyz
        //     zyyyz
        //     zzzzz
        //
        private static List<Vector2<int>> calculateRelativePositionsForRadius(int radius)
        {
            List<Vector2<int>> resultOffsets;

            Vector2<int> outwardIteratorOffsetUnbound;
            Vector2<int> one;

            outwardIteratorOffsetUnbound = new Vector2<int>();
            outwardIteratorOffsetUnbound.x = 0;
            outwardIteratorOffsetUnbound.y = 0;

            one = new Vector2<int>();
            one.x = 1;
            one.y = 1;

            resultOffsets = new List<Vector2<int>>();


            for (; ; )
            {
                Vector2<int> iteratorOffsetBoundMin;
                Vector2<int> iteratorOffsetBoundMax;
                int x, y;

                if (-outwardIteratorOffsetUnbound.x > radius)
                {
                    break;
                }



                iteratorOffsetBoundMin = outwardIteratorOffsetUnbound;
                iteratorOffsetBoundMax = outwardIteratorOffsetUnbound.getScaled(-1) + one;

                for (y = (int)(iteratorOffsetBoundMin.y); y < iteratorOffsetBoundMax.y; y++)
                {
                    for (x = (int)(iteratorOffsetBoundMin.x); x < iteratorOffsetBoundMax.x; x++)
                    {
                        // just add the border
                        if( y == (int)(iteratorOffsetBoundMin.y) || y == iteratorOffsetBoundMax.y - 1 || x ==  (int)(iteratorOffsetBoundMin.x) || x == iteratorOffsetBoundMax.x - 1 )
                        {
                            Vector2<int> newOffset;

                            newOffset = new Vector2<int>();
                            newOffset.x = x;
                            newOffset.y = y;

                            resultOffsets.Add(newOffset);
                        }
                        

                    }
                }

                outwardIteratorOffsetUnbound.x--;
                outwardIteratorOffsetUnbound.y--;
            }

            return resultOffsets;
        }

        private int[] convertRelativePositionsToArray(List<Vector2<int>> relativePositions)
        {
            int[] resultArray;
            int i;

            resultArray = new int[relativePositions.Count * 2];

            for (i = 0; i < relativePositions.Count; i++ )
            {
                resultArray[i * 2 + 0] = relativePositions[i].x;
                resultArray[i * 2 + 1] = relativePositions[i].y;
            }

            return resultArray;
        }

        private string getOpenClSource()
        {
            string programLocation = Assembly.GetEntryAssembly().Location;

            string pathToLoad = Path.Combine(Path.GetDirectoryName(programLocation), "..\\..\\", "ComputationBackend\\OpenCl\\src\\FindNearestPoint.cl");

            string openClSource = File.ReadAllText(pathToLoad);

            return openClSource;
        }

        public Vector2<int>[] inputPositions;
        public Vector2<int>[] outputPositions;
        public bool[] foundNewPositions;
        public Map2d<bool> inputMap;

        // is actually bool
        private OpenCL.Net.IMem<int> bufferForInputMap;
        
        private OpenCL.Net.IMem<int> bufferForRelativePositions;

        private int numberOfAllocatedInputAndOutputPositions;
        private OpenCL.Net.IMem<int> bufferForInputPositions;
        private OpenCL.Net.IMem<int> bufferForOutputPositions;

        // is actually bool
        private OpenCL.Net.IMem<int> bufferForFoundNewPosition;

        private OpenCL.Net.Program program;
        private OpenCL.Net.Kernel kernelNearestPoint;
    }
}
