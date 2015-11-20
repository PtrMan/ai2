using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartialOrderedSet
{

    // this is outdated, old and slow
    /** 
     * 
     * This algorithm tries to minimize the energy of a valid, sorted permutation further without violating the constrains.
     * IT NEEDS A VALID CAUSAL SET
     * 
     * for now we try to minimize the energy continiously without storing any other versions like in genetic algorithms
     * TODO< enableable feature for the future? >
     */
    class NetworkAlgorithm
    {
        public class Constraint
        {
            public int preiorElement;
            public int postierElement;
        }

        static public void doWork(uint iterations, List<Constraint> constraints, List<int> workingPermutation, Random random)
        {
            uint currentIteration;
            int minimalEnergy;

            
            // TODO LATER< use a index for all relations to quickly look it up >

            minimalEnergy = getEnergy(workingPermutation, constraints);

            for( currentIteration = 0; currentIteration < iterations; currentIteration++ )
            {
                bool swapViolatesAnyConstraint;
                int indexBefore;
                int indexAfter;
                int elementBefore;
                int elementAfter;
                List<Constraint> relevantConstraints;
                int currentEnergy;

                // choose two random indices
                chooseTwoIndepentNumbers(out indexBefore, out indexAfter, random, workingPermutation.Count);

                elementBefore = workingPermutation[indexBefore];
                elementAfter = workingPermutation[indexAfter];

                // get all relations assoziated with the two chosen elements
                // TODO LATER< use some sort of index >
                // TODO
                relevantConstraints = constraints;

                // do swap
                swap(indexAfter, indexBefore, workingPermutation);

                // check if the swap violated any relations
                // if not we do the swap
                swapViolatesAnyConstraint = violatedConstraints(workingPermutation, constraints);
                if( swapViolatesAnyConstraint )
                {
                    // swap back
                    swap(indexAfter, indexBefore, workingPermutation);

                    continue;
                }

                // we are here if the swap doesn't violate any constraints

                

                

                // TODO LATER< maybe we could calculate somehow only the energychange? >
                currentEnergy = getEnergy(workingPermutation, constraints);

                // swap back if the energy is higher >
                if( currentEnergy > minimalEnergy )
                {
                    swap(indexAfter, indexBefore, workingPermutation);
                }
                else if( currentEnergy != minimalEnergy )
                {
                    minimalEnergy = currentEnergy;

                    Console.Write("reduced energy to ");
                    Console.WriteLine(currentEnergy);
                }
            }
        }

        static private void chooseTwoIndepentNumbers(out int indexBefore, out int indexAfter, Random random, int numberOfElements)
        {
            for(;;)
            {
                indexBefore = random.Next(numberOfElements);
                indexAfter = random.Next(numberOfElements);

                if( indexAfter != indexBefore )
                {
                    break;
                }
            }
        }

        /*
        static private bool violatesConstraints(int preiorElement, int postierElement, List<Constraint> relevantRelations)
        {
            // TODO LATER< use any form of an index >

            Console.Write("violatesConstraints() ");
            Console.Write(preiorElement);
            Console.Write(postierElement);
            Console.WriteLine("");

            foreach( Constraint iterationRelation in relevantRelations )
            {
                Console.Write(iterationRelation.preiorElement);
                Console.Write(" ");
                Console.WriteLine(iterationRelation.postierElement);

                if( iterationRelation.preiorElement == postierElement && iterationRelation.postierElement == preiorElement )
                {
                    return true;
                }
            }

            return false;
        }*/

        static private void swap(int indexA, int indexB,  List<int> workingPermutation)
        {
            int oldValue;

            oldValue = workingPermutation[indexA];
            workingPermutation[indexA] = workingPermutation[indexB];
            workingPermutation[indexB] = oldValue;
        }

        // TOD LATER< we need a way to calculate the energy efficient >
        static private int getEnergy(List<int> workingPermutation, List<Constraint> constraints)
        {
            int energy;

            energy = 0;

            foreach(Constraint iterationConstraint in constraints)
            {
                int preiorIndex;
                int posteriorIndex;

                // search for the index of the preior
                for( preiorIndex = 0; preiorIndex < workingPermutation.Count; preiorIndex++ )
                {
                    if( workingPermutation[preiorIndex] == iterationConstraint.preiorElement )
                    {
                        break;
                    }
                }

                for( posteriorIndex = preiorIndex+1; posteriorIndex < workingPermutation.Count; posteriorIndex++ )
                {
                    if (workingPermutation[posteriorIndex] == iterationConstraint.postierElement)
                    {
                        break;
                    }
                }

                energy += (posteriorIndex - preiorIndex - 1);
            }

            return energy;
        }

        static private bool violatedConstraints(List<int> workingPermutation, List<Constraint> constraints)
        {
            foreach (Constraint iterationConstraint in constraints)
            {
                int preiorIndex;
                int posteriorIndex;

                // search for the index of the preior
                for (preiorIndex = 0; preiorIndex < workingPermutation.Count; preiorIndex++)
                {
                    if (workingPermutation[preiorIndex] == iterationConstraint.preiorElement)
                    {
                        break;
                    }
                }

                bool orderIsCorrect;

                orderIsCorrect = false;

                for (posteriorIndex = preiorIndex + 1; posteriorIndex < workingPermutation.Count; posteriorIndex++)
                {
                    if (workingPermutation[posteriorIndex] == iterationConstraint.postierElement)
                    {
                        orderIsCorrect = true;
                        break;
                    }
                }

                if( !orderIsCorrect )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
