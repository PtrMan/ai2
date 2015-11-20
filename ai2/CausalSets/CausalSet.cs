using System;
using System.Collections.Generic;

namespace CausalSets
{
    public class Constraint
    {
        public int preiorElement;
        public int postierElement;
    }

    /**
     * helper methods/functions for all causal sets
     * 
     * 
     */
    class CausalSet
    {
        

        // calculates the energy / functional of the permutation
        static public int calculateFunctional(List<int> permutation, List<Constraint> constraints)
        {
 
            int energy;

            energy = 0;

            foreach (Constraint iterationConstraint in constraints)
            {
                int preteriorIndex;
                int posteriorIndex;

                for (preteriorIndex = 0; preteriorIndex < permutation.Count; preteriorIndex++)
                {
                    if (iterationConstraint.preiorElement == permutation[preteriorIndex])
                    {
                        break;
                    }
                }

                for (posteriorIndex = preteriorIndex + 1; posteriorIndex < permutation.Count; posteriorIndex++)
                {
                    if (iterationConstraint.postierElement == permutation[posteriorIndex])
                    {
                        break;
                    }
                }

                energy += (posteriorIndex - preteriorIndex);
            }

            return energy;
        }
    }
}
