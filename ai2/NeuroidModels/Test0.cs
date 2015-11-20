using System;
using System.Collections.Generic;

namespace NeuroidModels
{
    class Test0 : NeuralNetworks.Neuroids.Neuroid<float, int>.IUpdate
    {
        public Test0(int latencyAfterActivation, float randomFiringPropability)
            {
                this.latencyAfterActivation = latencyAfterActivation;
                this.randomFiringPropability = randomFiringPropability;
            }

        

        public void calculateUpdateFunction(NeuralNetworks.Neuroids.Neuroid<float, int>.NeuroidGraphElement neuroid, List<int> updatedMode, List<float> updatedWeights)
        {
            neuroid.nextFiring = neuroid.isStimulated();

            if (neuroid.nextFiring)
            {
                neuroid.remainingLatency = latencyAfterActivation;
            }
            else
            {
                bool isFiring;

                isFiring = false; // (float)random.NextDouble() < randomFiringPropability;

                neuroid.nextFiring = isFiring;
            }
        }

        public void initialize(NeuralNetworks.Neuroids.Neuroid<float, int>.NeuroidGraphElement neuroid, List<int> parentIndices, List<int> updatedMode, List<float> updatedWeights)
        {
            neuroid.threshold = 1.0f;
        }
        
        private Random random = new Random();

        private int latencyAfterActivation;
        private float randomFiringPropability;

    }
    
}
