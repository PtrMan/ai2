using System;
using System.Collections.Generic;

// for debugging
using System.Drawing;


namespace ComputationBackend.cs
{
    class OperatorNeuroidVision
    {
        // TODO< angle calculation is for sure a bit wrong >

        /*
         * input signals for the first layer network 
         * * kernel
         * * direction 0*
         * ...
         * 
         */

        public struct Configuration
        {
            public Misc.Vector2<int> imageSize;

            public float cornerThreshold;
            public float edgeThreshold;

            public int directionCount;

            public int directionSampleDistance;

            public int layer0NeuroidLatencyAfterFiring;
            public float layer0NeuroidRandomFiringPropability;
        }

        private struct GaborKernelSettings
        {
            public int kernelWidth;
            public float kernelLamda;
            public float kernelPhaseOffset;
            public float kernelSpartialRatioAspect;
        }

        private class DirectionDescriptor
        {
            public Map2d<float> convolutionResult;
            public OperatorGaborFilter gaborFilter;

            public List<DirectionalNeuronDescriptor> directionNeuronDescriptors = new List<DirectionalNeuronDescriptor>();
        }

        private class DirectionalNeuronDescriptor
        {
            public int inputNeuronIndex;
            public int internalNeuronIndex; /** \brief neuron which gets activated by input and other neurons */

            public int kernelNeuronIndexA;
            public int kernelNeuronIndexB;

            // sample position in image, absolute
            public Misc.Vector2<int> samplePosition;
        }

        public bool[] getNeuroidActivation()
        {
            return neuroidLayer0.getActiviationOfNeurons();
        }

        public void initialize(OpenCl.ComputeContext computeContext)
        {
            initializeTemporaryMaps();
            initializeGaborFilters();
            initializeRadialKernel(computeContext);
            initializeEdgeConnections();
            initializeNeuroidNetworkForLayer0();
        }

        public void calculate(ComputationBackend.OpenCl.ComputeContext computeContext)
        {
            calculateMaps(computeContext);
            calculateNeuroids(computeContext);
            debugIntoDebugMap();
        }

        private void calculateNeuroids(ComputationBackend.OpenCl.ComputeContext computeContext)
        {
            int updateI;

            for( updateI = 0; updateI < 1; updateI++ )
            {
                neuroidLayer0.timestep();

                // debug activation
                bool[] activationOfNeurons = neuroidLayer0.getActiviationOfNeurons();

                int neuronI;

                
                for( neuronI = getNumberOfNeuronsForLayer0(); neuronI < activationOfNeurons.Length; neuronI++ )
                {
                    if( activationOfNeurons[neuronI] )
                    {
                        System.Console.WriteLine(neuronI);
                    }
                }
            }
        }

        private void calculateMaps(ComputationBackend.OpenCl.ComputeContext computeContext)
        {
            int convolutionI;
            int radialKernelI;

            // input map for the radial kernel calculations
            Map2d<float> inputForRadialKernel;

            // indicates if the kernel neuron is active for the given radialKernel
            bool[] radialKernelNeuronActive;

            for( convolutionI = 0; convolutionI < directionDescriptors.Length; convolutionI++ )
            {
                directionDescriptors[convolutionI].gaborFilter.inputMap = inputImage;
                directionDescriptors[convolutionI].gaborFilter.calculate(computeContext);
                
                //gaborFilters[convolutionI].outputMap
            }

            // TODO< this can be parallelized >
            operatorMax.inputA = new Map2d<float>((uint)configuration.imageSize.x, (uint)configuration.imageSize.y);

            for( convolutionI = 0; convolutionI < directionDescriptors.Length; convolutionI++ )
            {
                operatorMax.inputB = directionDescriptors[convolutionI].gaborFilter.outputMap;
                operatorMax.calculate(computeContext);

                operatorMax.inputA = operatorMax.result;
            }

            inputForRadialKernel = operatorMax.inputA;


            radialKernel.inputMap = inputForRadialKernel;
            radialKernel.calculate(computeContext);

            radialKernelNeuronActive = new bool[usedLengthOfRadialKernelArray];
            for( radialKernelI = 0; radialKernelI < usedLengthOfRadialKernelArray; radialKernelI++ )
            {
                if( radialKernel.kernelResults[radialKernelI] > configuration.cornerThreshold )
                {
                    radialKernelNeuronActive[radialKernelI] = true;
                }
            }

            // transfer radial kernel activation to neuroid network
            for( radialKernelI = 0; radialKernelI < usedLengthOfRadialKernelArray; radialKernelI++ )
            {
                neuroidLayer0.input[radialKernelI] = radialKernelNeuronActive[radialKernelI];
            }

            int neuroidInputOffset = usedLengthOfRadialKernelArray;

            int directionDescriptorI;

            for (directionDescriptorI = 0; directionDescriptorI < directionDescriptors.Length; directionDescriptorI++)
            {
                int directionNeuronDescriptorI;

                // read the convolution map and decide if it is above threashold
                // write information into neuroid network
                for( directionNeuronDescriptorI = 0; directionNeuronDescriptorI < directionDescriptors[directionDescriptorI].directionNeuronDescriptors.Count; directionNeuronDescriptorI++ )
                {
                    Misc.Vector2<int> samplePosition;
                    float gaborKernelValue;
                    bool samplePositionActive;

                    DirectionalNeuronDescriptor iterationDirectionalNeuronDescriptor = directionDescriptors[directionDescriptorI].directionNeuronDescriptors[directionNeuronDescriptorI];

                    samplePosition = iterationDirectionalNeuronDescriptor.samplePosition - new Misc.Vector2<int>(gaborKernelKernelWidth/2, gaborKernelKernelWidth/2);

                    gaborKernelValue = directionDescriptors[directionDescriptorI].gaborFilter.outputMap.readAt(samplePosition.x, samplePosition.y);
                    samplePositionActive = gaborKernelValue > configuration.edgeThreshold;

                    neuroidLayer0.input[neuroidInputOffset + directionNeuronDescriptorI] = samplePositionActive;
                }

                neuroidInputOffset += directionDescriptors[directionDescriptorI].directionNeuronDescriptors.Count;
            }


        }

        private void initializeTemporaryMaps()
        {
            int directionDescriptorI;
            
            directionDescriptors = new DirectionDescriptor[configuration.directionCount];

            for( directionDescriptorI = 0; directionDescriptorI < configuration.directionCount; directionDescriptorI++ )
            {
                directionDescriptors[directionDescriptorI] = new DirectionDescriptor();
            }

            for( directionDescriptorI = 0; directionDescriptorI < configuration.directionCount; directionDescriptorI++ )
            {
                directionDescriptors[directionDescriptorI].convolutionResult = new Map2d<float>((uint)configuration.imageSize.x, (uint)configuration.imageSize.y);
            }
        }

        private void initializeRadialKernel(ComputationBackend.OpenCl.ComputeContext computeContext)
        {
            int i;
            int realLengthOfKernelArray;

            usedLengthOfRadialKernelArray = (((int)configuration.imageSize.x - kernelWidth) / radialKernelDistance) * (((int)configuration.imageSize.y - kernelWidth) / radialKernelDistance);
            realLengthOfKernelArray = usedLengthOfRadialKernelArray + (32 - (usedLengthOfRadialKernelArray % 32));

            radialKernel.kernelPositions = new Misc.Vector2<int>[realLengthOfKernelArray];

            for( i = 0; i < radialKernel.kernelPositions.Length; i++ )
            {
                radialKernel.kernelPositions[i] = new Misc.Vector2<int>();
            }

            
            for( i = 0; i < usedLengthOfRadialKernelArray; i++ )
            {
                radialKernel.kernelPositions[i].x = (i % (((int)configuration.imageSize.x - kernelWidth) / radialKernelDistance)) * radialKernelDistance;
                radialKernel.kernelPositions[i].y = (i / (((int)configuration.imageSize.x - kernelWidth) / radialKernelDistance)) * radialKernelDistance;

                System.Diagnostics.Debug.Assert(i == getIndexOfKernelAtPosition(radialKernel.kernelPositions[i]));
            }

            radialKernel.createKernel(kernelWidth);

            radialKernel.initialize(computeContext, radialKernel.kernelPositions.Length, configuration.imageSize);
        }

        private void initializeGaborFilters()
        {
            GaborKernelSettings gaborKernelSettings;
            int directionDescriptorI;

            
            gaborKernelKernelWidth = gaborKernelSettings.kernelWidth = 8;
            gaborKernelSettings.kernelLamda = 5.0f/8.0f;
            gaborKernelSettings.kernelPhaseOffset = (float)System.Math.PI * 0.5f;
            gaborKernelSettings.kernelSpartialRatioAspect = 1.0f;// 0.4f;

            for (directionDescriptorI = 0; directionDescriptorI < configuration.directionCount; directionDescriptorI++)
            {
                float kernelPhi;

                kernelPhi = ((1.0f / (float)configuration.directionCount) * (float)directionDescriptorI) * (float)System.Math.PI;

                directionDescriptors[directionDescriptorI].gaborFilter = new OperatorGaborFilter();
                directionDescriptors[directionDescriptorI].gaborFilter.kernelPhi = kernelPhi;
                directionDescriptors[directionDescriptorI].gaborFilter.kernelLamda = gaborKernelSettings.kernelLamda;
                directionDescriptors[directionDescriptorI].gaborFilter.kernelPhaseOffset = gaborKernelSettings.kernelPhaseOffset;
                directionDescriptors[directionDescriptorI].gaborFilter.kernelWidth = gaborKernelSettings.kernelWidth;
                directionDescriptors[directionDescriptorI].gaborFilter.kernelSpartialRatioAspect = gaborKernelSettings.kernelSpartialRatioAspect;
                directionDescriptors[directionDescriptorI].gaborFilter.calculateKernel();
            }
        }

        private void initializeNeuroidNetworkForLayer0()
        {
            int neuroidInputLength;
            int kernelI;
            int directionI;
            int cellI;
            int directionDescriptorI;

            // contains base index of direction cells
            int directionBaseIndex;
            int directionDescriptorsI;

            int numberOfNeuronsForLayer0;

            neuroidInputLength = 0;
            neuroidInputLength += usedLengthOfRadialKernelArray;

            for (directionDescriptorI = 0; directionDescriptorI < configuration.directionCount; directionDescriptorI++)
            {
                neuroidInputLength += directionDescriptors[directionDescriptorI].directionNeuronDescriptors.Count;
            }

            neuroidLayer0.input = new bool[neuroidInputLength];



            neuroidLayer0.update = new NeuroidLevel0(configuration.layer0NeuroidLatencyAfterFiring, configuration.layer0NeuroidRandomFiringPropability);

            // initialize neurons for layer 0
            numberOfNeuronsForLayer0 = getNumberOfNeuronsForLayer0();
            neuroidLayer0.allocateNeurons(numberOfNeuronsForLayer0 + neuroidInputLength, neuroidInputLength);

            int noninputNeuroidsOffset;

            noninputNeuroidsOffset = neuroidInputLength;

            // kernel input to kernel neurons
            for( kernelI = 0; kernelI < usedLengthOfRadialKernelArray; kernelI++ )
            {
                System.Diagnostics.Debug.Assert(kernelI < neuroidInputLength);

                neuroidLayer0.addConnection(kernelI, noninputNeuroidsOffset + kernelI, 1.0f);
            }

            directionBaseIndex = usedLengthOfRadialKernelArray;

            for (directionDescriptorsI = 0; directionDescriptorsI < configuration.directionCount; directionDescriptorsI++)
            {
                // direction input to direction neurons (over length 1, direct)
                for (directionI = 0; directionI < directionDescriptors[directionDescriptorsI].directionNeuronDescriptors.Count; directionI++)
                {
                    System.Diagnostics.Debug.Assert(directionI + directionBaseIndex < neuroidInputLength);

                    neuroidLayer0.addConnection(directionI + directionBaseIndex, noninputNeuroidsOffset + directionI + directionBaseIndex, 1.0f);

                    directionDescriptors[directionDescriptorsI].directionNeuronDescriptors[directionI].internalNeuronIndex = noninputNeuroidsOffset + directionI + directionBaseIndex;
                }

                // connection between direction cell and kernel
                for (cellI = 0; cellI < directionDescriptors[directionDescriptorsI].directionNeuronDescriptors.Count; cellI++)
                {
                    DirectionalNeuronDescriptor currentDirectionalNeuronDescriptor;

                    currentDirectionalNeuronDescriptor = directionDescriptors[directionDescriptorsI].directionNeuronDescriptors[cellI];

                    neuroidLayer0.addTwoWayConnection(currentDirectionalNeuronDescriptor.internalNeuronIndex, noninputNeuroidsOffset + currentDirectionalNeuronDescriptor.kernelNeuronIndexA, 1.0f);
                    neuroidLayer0.addTwoWayConnection(currentDirectionalNeuronDescriptor.internalNeuronIndex, noninputNeuroidsOffset + currentDirectionalNeuronDescriptor.kernelNeuronIndexB, 1.0f);
                }

                directionBaseIndex += directionDescriptors[directionDescriptorsI].directionNeuronDescriptors.Count;
            }


            neuroidLayer0.initialize();
        }

        private int getNumberOfNeuronsForLayer0()
        {
            int numberOfNeurons;
            int directionDescriptorI;

            numberOfNeurons = 0;
            numberOfNeurons += usedLengthOfRadialKernelArray;

            for (directionDescriptorI = 0; directionDescriptorI < configuration.directionCount; directionDescriptorI++)
            {
                numberOfNeurons += directionDescriptors[directionDescriptorI].directionNeuronDescriptors.Count;
            }

            return numberOfNeurons;
        }

        /**
         * calculate the positions of the read positions (in image space) of the direction and the connections to the 
         * radial kernel elements (with indices)
         * 
         * 
         */
        private void initializeEdgeConnections()
        {
            float directionDiameter;
            float directionRadius;

            int centerX, centerY;

            int roundUpDirectionRadiusAsInt;

            int directionDescriptorIndex;
            int directionNeuronIndex;

            directionDescriptorIndex = 0;

            // is not reseted for each direction
            directionNeuronIndex = usedLengthOfRadialKernelArray;


            // multiplied with two because we want to span two grids
            directionDiameter = (float)System.Math.Sqrt(5.0f + 5.0f) * 2.0f;
            directionRadius = directionDiameter * 0.5f;

            // + 1 because else it is too low
            roundUpDirectionRadiusAsInt = (int)System.Math.Round(directionRadius, MidpointRounding.AwayFromZero) + 1;

            for (directionDescriptorIndex = 0; directionDescriptorIndex < configuration.directionCount; directionDescriptorIndex++)
            {
                for (centerX = roundUpDirectionRadiusAsInt; centerX < configuration.imageSize.x - roundUpDirectionRadiusAsInt - kernelWidth; centerX += configuration.directionSampleDistance)
                {
                    for (centerY = roundUpDirectionRadiusAsInt; centerY < configuration.imageSize.y - roundUpDirectionRadiusAsInt - kernelWidth; centerY += configuration.directionSampleDistance)
                    {
                        float relativeXOfAngle, relativeYOfAngle;

                        int kernelIndexForA, kernelIndexForB;

                        DirectionalNeuronDescriptor createdDirectionalNeuronDescriptor;

                        Misc.Vector2<float> positionAAsFloat;
                        Misc.Vector2<float> positionBAsFloat;

                        Misc.Vector2<int> roundedPositionA;
                        Misc.Vector2<int> roundedPositionB;

                        calculateRelativeDirectionalOffset(directionDescriptorIndex, directionRadius, out relativeXOfAngle, out relativeYOfAngle);

                        positionAAsFloat = new Misc.Vector2<float>();
                        positionAAsFloat.x = (float)centerX + relativeXOfAngle;
                        positionAAsFloat.y = (float)centerY + relativeYOfAngle;

                        positionBAsFloat = new Misc.Vector2<float>();
                        positionBAsFloat.x = (float)centerX - relativeXOfAngle;
                        positionBAsFloat.y = (float)centerY - relativeYOfAngle;

                        roundedPositionA = new Misc.Vector2<int>();
                        roundedPositionA.x = ((int)positionAAsFloat.x / radialKernelDistance) * radialKernelDistance;
                        roundedPositionA.y = ((int)positionAAsFloat.y / radialKernelDistance) * radialKernelDistance;

                        roundedPositionB = new Misc.Vector2<int>();
                        roundedPositionB.x = ((int)positionBAsFloat.x / radialKernelDistance) * radialKernelDistance;
                        roundedPositionB.y = ((int)positionBAsFloat.y / radialKernelDistance) * radialKernelDistance;

                        kernelIndexForA = getIndexOfKernelAtPosition(roundedPositionA);
                        kernelIndexForB = getIndexOfKernelAtPosition(roundedPositionB);

                        createdDirectionalNeuronDescriptor = new DirectionalNeuronDescriptor();
                        createdDirectionalNeuronDescriptor.kernelNeuronIndexA = kernelIndexForA;
                        createdDirectionalNeuronDescriptor.kernelNeuronIndexB = kernelIndexForB;
                        createdDirectionalNeuronDescriptor.inputNeuronIndex = directionNeuronIndex;
                        createdDirectionalNeuronDescriptor.samplePosition = new Misc.Vector2<int>(centerX, centerY);
                        directionDescriptors[directionDescriptorIndex].directionNeuronDescriptors.Add(createdDirectionalNeuronDescriptor);

                        directionNeuronIndex++;
                    }
                }
            }
        }

        private int getIndexOfKernelAtPosition(Misc.Vector2<int> position)
        {
            int indexX, indexY;
            int indexWidth;

            System.Diagnostics.Debug.Assert(position.x >= 0 && position.x < configuration.imageSize.x - kernelWidth);
            System.Diagnostics.Debug.Assert(position.y >= 0 && position.y < configuration.imageSize.y - kernelWidth);

            indexX = (position.x) / radialKernelDistance;
            indexY = (position.y) / radialKernelDistance;

            indexWidth = (configuration.imageSize.x - kernelWidth) / radialKernelDistance;

            return indexX + indexY * indexWidth;
        }


        private void debugIntoDebugMap()
        {
            bool[] neuroidActivation;
            float directionDiameter;
            float directionRadius;

            Bitmap debugBitmap = new Bitmap(configuration.imageSize.x, configuration.imageSize.y);
            Graphics drawingGraphics = Graphics.FromImage(debugBitmap);
            Pen redPen = new Pen(Brushes.Red);
            Pen greenPen = new Pen(Brushes.Green);

            directionDiameter = (float)System.Math.Sqrt(5.0f + 5.0f) * 2.0f;
            directionRadius = directionDiameter * 0.5f;

            neuroidActivation = neuroidLayer0.getActiviationOfNeurons();

            debugOutput = new Map2d<Misc.ColorRgb>((uint)configuration.imageSize.x, (uint)configuration.imageSize.y);

            // debug kernel activation

            int neuroidInputLength;
            int directionDescriptorI;

            neuroidInputLength = 0;
            neuroidInputLength += usedLengthOfRadialKernelArray;

            for (directionDescriptorI = 0; directionDescriptorI < configuration.directionCount; directionDescriptorI++)
            {
                neuroidInputLength += directionDescriptors[directionDescriptorI].directionNeuronDescriptors.Count;
            }



            int noninputNeuroidsOffset = neuroidInputLength;

            
            int kernelI;

            for( kernelI = 0; kernelI < usedLengthOfRadialKernelArray; kernelI++ )
            {
                bool activation;
                Misc.Vector2<int> position;

                activation = neuroidActivation[noninputNeuroidsOffset + kernelI];

                if( activation )
                {
                    position = radialKernel.kernelPositions[kernelI];

                    drawingGraphics.DrawLine(greenPen, position.x, position.y, position.x + 2, position.y);
                }
            }
             


            // debug gabor activation

            //int directionDescriptorI;


            for (directionDescriptorI = 0; directionDescriptorI < configuration.directionCount; directionDescriptorI++)
            {
                float relativeX;
                float relativeY;
                Misc.Vector2<int> offsetDirection;

                calculateRelativeDirectionalOffset(directionDescriptorI, directionRadius, out relativeX, out relativeY);

                offsetDirection = new Misc.Vector2<int>((int)relativeX, (int)relativeY);

                foreach( DirectionalNeuronDescriptor iterationDirectionalNeuronDescriptor in directionDescriptors[directionDescriptorI].directionNeuronDescriptors )
                {
                    bool neuronActivation;

                    neuronActivation = neuroidActivation[iterationDirectionalNeuronDescriptor.internalNeuronIndex];

                    if( neuronActivation )
                    {
                        // draw debug line as the direction of the sampled gabor position

                        Misc.Vector2<int> samplePosition;
                        Misc.Vector2<int> positionLineBegin;
                        Misc.Vector2<int> positionLineEnd;
                        

                        samplePosition = iterationDirectionalNeuronDescriptor.samplePosition;

                        positionLineBegin = samplePosition + offsetDirection;
                        positionLineEnd = samplePosition - offsetDirection;

                        drawingGraphics.DrawLine(redPen, positionLineBegin.x, positionLineBegin.y, positionLineEnd.x, positionLineEnd.y);
                    }
                }
            }

            drawingGraphics.Flush();

            // transfer bitmap to map2d

            int x, y;

            for( x = 0; x < configuration.imageSize.x; x++ )
            {
                for( y = 0; y < configuration.imageSize.y; y++ )
                {
                    Color readColor;
                    Misc.ColorRgb convertedColor;

                    readColor = debugBitmap.GetPixel(x, y);
                    convertedColor = new Misc.ColorRgb((float)readColor.R / 255.0f, (float)readColor.G / 255.0f, (float)readColor.B / 255.0f);
                    debugOutput.writeAt(x, y, convertedColor);
                }
            }
        }

        private void calculateRelativeDirectionalOffset(int directionIndex, float directionRadius, out float relativeX, out float relativeY)
        {
            float angle;
            float sineOfAngle;
            float cosineOfAngle;

            angle = ((float)directionIndex / (float)(configuration.directionCount)) * ((float)System.Math.PI);

            sineOfAngle = (float)System.Math.Sin(angle);
            cosineOfAngle = (float)System.Math.Cos(angle);

            relativeX = sineOfAngle * directionRadius;
            relativeY = cosineOfAngle * directionRadius;
        }

        private class NeuroidLevel0 : NeuralNetworks.Neuroids.Neuroid<float, float>.IUpdate
        {
            public NeuroidLevel0(int latencyAfterActivation, float randomFiringPropability)
            {
                this.latencyAfterActivation = latencyAfterActivation;
                this.randomFiringPropability = randomFiringPropability;
            }

            

            public void calculateUpdateFunction(NeuralNetworks.Neuroids.Neuroid<float, float>.NeuroidGraphElement neuroid, List<float> updatedMode, List<float> updatedWeights)
            {
                neuroid.nextFiring = neuroid.isStimulated();

                if( neuroid.nextFiring )
                {
                    int x = 0;
                }
                else
                {
                    bool isFiring;

                    isFiring = (float)random.NextDouble() < randomFiringPropability;

                    neuroid.nextFiring = isFiring;
                }

                if (neuroid.nextFiring)
                {
                    neuroid.remainingLatency = latencyAfterActivation;
                }
                
            }

            public void initialize(NeuralNetworks.Neuroids.Neuroid<float, float>.NeuroidGraphElement neuroid, List<int> parentIndices, List<float> updatedMode, List<float> updatedWeights)
            {
                int weightI;
                List<float> newUpdatedWeights;

                updatedMode = neuroid.mode;

                //newUpdatedWeights = new List<float>();

                /*
                for( weightI = 0; weightI < neuroid.weights.Count; weightI++ )
                {
                    newUpdatedWeights.Add(1.0f);
                }
                */

                // + 1 because else the neuron gets actived from the input from the input neurons
                neuroid.threshold = (float)(1.5f);

                //updatedWeights = newUpdatedWeights;
            }

            private Random random = new Random();

            private int latencyAfterActivation;
            private float randomFiringPropability;
        }


        public Configuration configuration;

        public Map2d<float> inputImage;

        // just for debugging
        public Map2d<Misc.ColorRgb> debugOutput;

        private DirectionDescriptor[] directionDescriptors;

        private ComputationBackend.OpenCl.OperatorRadialKernel radialKernel = new OpenCl.OperatorRadialKernel();

        // used number of kernel positions
        private int usedLengthOfRadialKernelArray;

        private int gaborKernelKernelWidth;

        // distance between the radialKernel kernels
        private int radialKernelDistance = 2;

        private int kernelWidth = 3;

        private NeuralNetworks.Neuroids.Neuroid<float, float> neuroidLayer0 = new NeuralNetworks.Neuroids.Neuroid<float, float>();

        private OperatorMax operatorMax = new OperatorMax();
    }
}
