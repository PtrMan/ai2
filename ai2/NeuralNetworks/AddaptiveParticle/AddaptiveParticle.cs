using System;
using System.Collections.Generic;

// for testing timing
using System.Diagnostics;

namespace NeuralNetworks.AddaptiveParticle
{
    /**
     * 
     * self organizing map like the kohonen network
     * 
     * each neuron is added on a seperated pass
     * there is a rejecting force between the allready fixed neurons and the new neuron
     * if the distance of a new wandering neuron to the goal is larger than a threshold it gets assigned to the nearest allready existing neuron because it is similar to it
     * 
     */
    class AddaptiveParticle
    {
        public class Neuron
        {
            public float[] position;
        }

        public AddaptiveParticle(float repelMultiplicator, float fuseDistance, int movementIterations)
        {
            this.repelMultiplicator = repelMultiplicator;
            this.fuseDistance = fuseDistance;
            this.movementIterations = movementIterations;
        }

        public void remember(float[] position, out int neuronResultIndex)
        {
            int iteration;
            int neuronI;

            float[] currentPosition;

            Stopwatch stopwatch;
            stopwatch = new Stopwatch();
            stopwatch.Restart();

            currentPosition = (float[])position.Clone();

            for( iteration = 0; iteration < movementIterations; iteration++ )
            {
                float[] force;
                
                // it gets pulled toward the position/destination
                force = subtractArray(ref currentPosition, ref position);

                foreach( Neuron iterationNeuron in neurons )
                {
                    float distance;
                    float forceMagnitude;
                    float[] iterationForce;

                    distance = distanceDelegate(ref iterationNeuron.position, ref currentPosition);

                    // TODO< calculate force >

                    forceMagnitude = (1.0f / (distance * distance)) * repelMultiplicator;

                    iterationForce = subtractArray(ref currentPosition, ref iterationNeuron.position);
                    normalizeArray(ref iterationForce);
                    scaleArray(ref iterationForce, forceMagnitude);

                    force = addArray(ref force, ref iterationForce);
                }

                // move by force
                currentPosition = addArray(ref currentPosition, ref force);
            }

            // look if it can be captured by any neuron

            for( neuronI = 0; neuronI < neurons.Count; neuronI++ )
            {
                float distance = distanceDelegate(ref currentPosition, ref neurons[neuronI].position);

                if( distance < fuseDistance )
                {
                    neuronResultIndex = neuronI;
                    return;
                }
            }

            // if we are here it didn't got captured

            // so we create a new neuron and add it
            neurons.Add(new Neuron());
            neurons[neurons.Count - 1].position = currentPosition;

            stopwatch.Stop();

            System.Console.WriteLine("remeber took {0} us", (stopwatch.ElapsedTicks * 100) / 1000);

            neuronResultIndex = neurons.Count - 1;
            return;
        }

        static private void normalizeArray(ref float[] array)
        {
            float magnitude;
            float inverseMagnitude;
            int i;

            magnitude = magnitudeCartesian(ref array);
            System.Diagnostics.Debug.Assert(magnitude != 0.0f);
            
            inverseMagnitude = 1.0f / magnitude;

            for( i = 0; i < array.Length; i++ )
            {
                array[i] *= inverseMagnitude;
            }
        }

        static private void scaleArray(ref float[] array, float scale)
        {
            int i;

            for (i = 0; i < array.Length; i++)
            {
                array[i] *= scale;
            }
        }

        static private float magnitudeCartesian(ref float[] array)
        {
            float magnitude;
            int i;

            magnitude = 0.0f;

            for( i = 0; i < array.Length; i++ )
            {
                magnitude += (array[i] * array[i]);
            }
            
            return (float)System.Math.Sqrt(magnitude);
        }

        static private void fillZeroArray(ref float[] array)
        {
            int i;

            for( i = 0; i < array.Length; i++ )
            {
                array[i] = 0.0f;
            }
        }

        static private float[] subtractArray(ref float[] a, ref float[] b)
        {
            int i;

            float[] result;

            result = new float[a.Length];

            for( i = 0; i < a.Length; i++ )
            {
                result[i] = a[i] - b[i];
            }

            return result;
        }

        static private float[] addArray(ref float[] a, ref float[] b)
        {
            int i;

            float[] result;

            result = new float[a.Length];

            for (i = 0; i < a.Length; i++)
            {
                result[i] = a[i] + b[i];
            }

            return result;
        }



        // used by the self organizing map to calculate the distance
        private static float getDistanceOfPatch(ref float[] a, ref float[] b)
        {
            // length of all data of one color
            int colorspaceLength;
            int i;
            float distanceComponentA;
            float distanceComponentB;

            colorspaceLength = a.Length / 2;

            distanceComponentA = 0.0f;
            distanceComponentB = 0.0f;

            for (i = 0; i < colorspaceLength; i++)
            {
                distanceComponentA = distanceComponentA + (a[i] - b[i]) * (a[i] - b[i]);
            }

            for (i = colorspaceLength; i < a.Length; i++)
            {
                distanceComponentB = distanceComponentB + (a[i] - b[i]) * (a[i] - b[i]);
            }

            distanceComponentA = (float)System.Math.Sqrt(distanceComponentA);
            distanceComponentB = (float)System.Math.Sqrt(distanceComponentB);

            return distanceComponentA + distanceComponentB;
        }

        private static float distanceDelegate(ref float[] a, ref float[] b)
        {
            return getDistanceOfPatch(ref a, ref b);
        }

        private int movementIterations;
        private float repelMultiplicator;
        private float fuseDistance;

        public delegate float DelegateCalculateDistance(ref float[] a, ref float[] b);
        
        //public DelegateCalculateDistance distanceDelegate; // delegate to calculate the distance between neurons

        public List<Neuron> neurons = new List<Neuron>();
    }
}
