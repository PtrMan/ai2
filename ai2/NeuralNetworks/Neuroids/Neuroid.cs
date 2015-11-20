using System;
using System.Collections.Generic;

namespace NeuralNetworks.Neuroids
{
    /**
     * 
     * Idea of the neural network is from the book "Circuits of the mind"
     * 
     */
    class Neuroid<Weighttype, ModeType>
    {
        public class NeuroidGraphElement
        {
            public bool isInputNeuroid = false; /** \brief indicates if this neuroid is an input neuroid, means that it gets its activation from outside and has no parent connections or theshold and so on */

            public Weighttype threshold;
            public List<ModeType> mode = new List<ModeType>();
            public int state = 0; // startstate

            public bool firing = false;
            public bool nextFiring = false; /** \brief indicates if the neuron should fore on the next timestep, is updated by the update function */

            //public List<Weighttype> weights = new List<Weighttype>(); /* \brief weight for each children in the graph */


            public int remainingLatency = 0; /** \brief as long as this is > 0 its mode nor its weights can be changed */
            
            public Weighttype sumOfIncommingWeights;

            public bool isStimulated()
            {
                return (dynamic)sumOfIncommingWeights >= threshold;
            }

            public void updateFiring()
            {
                firing = nextFiring;
                nextFiring = false;
            }
        }

        public struct State
        {
            public int latency;
        }

        public interface IUpdate
        {
            void calculateUpdateFunction(NeuroidGraphElement neuroid, List<ModeType> updatedMode, List<Weighttype> updatedWeights);
            void initialize(NeuroidGraphElement neuroid, List<int> parentIndices, List<ModeType> updatedMode, List<Weighttype> updatedWeights);
        }

        public void initialize()
        {
            int neuronI;

            for( neuronI = 0; neuronI < neuroidsGraph.elements.Count; neuronI++ )
            {
                List<ModeType> modes;
                List<Weighttype> weights;

                bool thresholdValid;

                // a input neuroid doesn't have to be initialized
                if( neuroidsGraph.elements[neuronI].content.isInputNeuroid )
                {
                    continue;
                }

                modes = new List<ModeType>();
                weights = new List<Weighttype>();

                update.initialize(neuroidsGraph.elements[neuronI].content, neuroidsGraph.elements[neuronI].parentIndices, modes, weights);
                neuroidsGraph.elements[neuronI].content.mode = modes;
                //neuroidsGraph.elements[neuronI].content.weights = weights;

                thresholdValid = (dynamic)neuroidsGraph.elements[neuronI].content.threshold > default(Weighttype);

                System.Diagnostics.Debug.Assert(thresholdValid, "threshold must be greater than 0.0!");
            }
        }

        // just for debugging
        public void debugAllNeurons()
        {
            int neuronI;

            System.Console.WriteLine("===");

            for( neuronI = 0; neuronI < neuroidsGraph.elements.Count; neuronI++ )
            {
                NeuroidGraphElement neuroidGraphElement;

                neuroidGraphElement = neuroidsGraph.elements[neuronI].content;

                System.Console.WriteLine("neuron {0} isFiring {1}", neuronI, neuroidGraphElement.firing);
            }
        }

        public void timestep()
        {

            

            /*
            updateFiringForInputNeuroids();
            updateIncommingWeigthsForAllNeuroids();
             */
            

            // order is important, we first update input and then all neuroids
            updateFiringForInputNeuroids();
            updateFiringForAllNeuroids();


            updateIncommingWeigthsForAllNeuroids();

            updateNeuronStates();
            decreaseLatency();
        }

        public void addConnection(int a, int b, Weighttype weight)
        {
            System.Diagnostics.Debug.Assert(a >= 0);
            System.Diagnostics.Debug.Assert(b >= 0);
            
            neuroidsGraph.elements[a].childIndices.Add(b);
            neuroidsGraph.elements[a].childWeights.Add(weight);
            
            neuroidsGraph.elements[b].parentIndices.Add(a);
            
            //neuroidsGraph.elements[a].content.weights.Add(weight);
        }

        public void addTwoWayConnection(int a, int b, Weighttype weight)
        {
            addConnection(a, b, weight);
            addConnection(b, a, weight);
        }

        public bool[] getActiviationOfNeurons()
        {
            bool[] activationResult;
            int neuronI;

            activationResult = new bool[neuroidsGraph.elements.Count];

            for( neuronI = 0; neuronI < neuroidsGraph.elements.Count; neuronI++ )
            {
                activationResult[neuronI] = neuroidsGraph.elements[neuronI].content.firing;
            }

            return activationResult;
        }

        /** \brief reallocates the neurons
         * 
         * the neuronCount includes the count of the input neurons
         * 
         */
        public void allocateNeurons(int neuronCount, int inputCount)
        {
            int neuronI;

            allocatedInputNeuroids = inputCount;

            neuroidsGraph.elements.Clear();

            for( neuronI = 0; neuronI < neuronCount; neuronI++ )
            {
                neuroidsGraph.elements.Add(new Datastructures.DirectedGraph<NeuroidGraphElement, Weighttype>.Element(new NeuroidGraphElement()));
            }

            for (neuronI = 0; neuronI < inputCount; neuronI++)
            {
                neuroidsGraph.elements[neuronI].content.isInputNeuroid = true;
            }
        }

        private void updateNeuronStates()
        {
            foreach( Datastructures.DirectedGraph<NeuroidGraphElement, Weighttype>.Element iterationNeuron in neuroidsGraph.elements )
            {
                List<ModeType> updatedMode;
                List<Weighttype> updatedWeights;

                // input neuron doesn't have to be updated
                if( iterationNeuron.content.isInputNeuroid )
                {
                    continue;
                }

                // neurons with latency doesn't have to be updated
                if( iterationNeuron.content.remainingLatency > 0 )
                {
                    continue;
                }

                updatedMode = null;
                updatedWeights = null;
                
                update.calculateUpdateFunction(iterationNeuron.content, updatedMode, updatedWeights);

                iterationNeuron.content.mode = updatedMode;
                //iterationNeuron.content.weights = updatedWeights;
            }
        }

        private void updateIncommingWeigthsForAllNeuroids()
        {
            int iterationNeuronI;

            // add up the weights of the incomming edges
            for( iterationNeuronI = 0; iterationNeuronI < neuroidsGraph.elements.Count; iterationNeuronI++ )
            {
                Datastructures.DirectedGraph<NeuroidGraphElement, Weighttype>.Element iterationNeuron;
                dynamic sumOfWeightsOfThisNeuron;

                iterationNeuron = neuroidsGraph.elements[iterationNeuronI];

                // activation of a input neuron doesn't have to be calculated
                if( iterationNeuron.content.isInputNeuroid )
                {
                    continue;
                }

                sumOfWeightsOfThisNeuron = default(Weighttype);

                foreach( int iterationParentIndex in iterationNeuron.parentIndices )
                {
                    bool activation;

                    activation = neuroidsGraph.elements[iterationParentIndex].content.firing;
                    
                    if( activation )
                    {
                        Weighttype edgeWeight = neuroidsGraph.getEdgeWeight(iterationParentIndex, iterationNeuronI);

                        sumOfWeightsOfThisNeuron += edgeWeight;
                    }
                }

                iterationNeuron.content.sumOfIncommingWeights = sumOfWeightsOfThisNeuron;
            }
        }

        private void updateFiringForAllNeuroids()
        {
            foreach (Datastructures.DirectedGraph<NeuroidGraphElement, Weighttype>.Element iterationNeuron in neuroidsGraph.elements)
            {
                iterationNeuron.content.updateFiring();
            }
        }

        private void updateFiringForInputNeuroids()
        {
            int inputNeuroidI;

            System.Diagnostics.Debug.Assert(allocatedInputNeuroids == input.Length);

            for( inputNeuroidI = 0; inputNeuroidI < input.Length; inputNeuroidI++ )
            {
                neuroidsGraph.elements[inputNeuroidI].content.nextFiring = input[inputNeuroidI];
            }
        }

        private void decreaseLatency()
        {
            foreach (Datastructures.DirectedGraph<NeuroidGraphElement, Weighttype>.Element iterationNeuron in neuroidsGraph.elements)
            {
                if (iterationNeuron.content.remainingLatency > 0)
                {
                    iterationNeuron.content.remainingLatency--;
                }
            }
        }

        // input from outside
        // must be set and resized from outside
        public bool[] input;


        public IUpdate update;

        public State[] stateInformations;

        private Datastructures.DirectedGraph<NeuroidGraphElement, Weighttype> neuroidsGraph = new Datastructures.DirectedGraph<NeuroidGraphElement, Weighttype>();

        private int allocatedInputNeuroids;
    }
}
