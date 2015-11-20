using System;
using System.Collections.Generic;

namespace NeuroidModels.Visual.Layer0
{
    class Layer0
    {
        private class NeuroidUpdate : NeuralNetworks.Neuroids.Neuroid<float, int>.IUpdate
        {
            public void calculateUpdateFunction(NeuralNetworks.Neuroids.Neuroid<float, int>.NeuroidGraphElement neuroid, List<int> updatedMode, List<float> updatedWeights)
            {

            }

            public void initialize(NeuralNetworks.Neuroids.Neuroid<float, int>.NeuroidGraphElement neuroid, List<int> parentIndices, List<int> updatedMode, List<float> updatedWeights)
            {

            }
        }

        // for testing public
        public NeuralNetworks.Neuroids.Neuroid<float, int> neuroid = new NeuralNetworks.Neuroids.Neuroid<float, int>();
    }
}
