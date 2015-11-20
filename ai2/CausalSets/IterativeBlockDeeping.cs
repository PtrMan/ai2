using System;
using System.Collections.Generic;

namespace CausalSets
{
    /**
     * 
     * implements a algorithm where the causal set is iterativly permutated and compacted to blocks until no more blocks are found
     * 
     * 
     */
    class IterativeBlockDeeping
    {
        /**
         * 
         * the result is the result permutation
         * 
         * 
         */
        public List<int> doWork(List<int> inputPermutation, List<CausalSets.Constraint> inputConstraints)
        {
            List<int> workingPermutation;
            List<CausalSets.Constraint> workingConstraints;
            // stack for the translate infos used for reconstruction
            List<CausalSets.BlockResponsibility.TranslateInfo> translateInfoStack;

            workingPermutation = inputPermutation;
            workingConstraints = inputConstraints;

            translateInfoStack = new List<BlockResponsibility.TranslateInfo>();

            // first we order it and try to form blocks until no blocks can be formed
            for(;;)
            {
                List<int> reorderedWorkingPermutation;
                CausalSets.BlockResponsibility.TranslateInfo iterationBlockTranslatedInfo;
                bool newBlocksWereFormed;

                System.Console.WriteLine("IterativeBlockDeeping.doWork() length of permutation is {0}", workingPermutation.Count);

                reorderedWorkingPermutation = causalNeuronBasedAlgorithm.doWork(workingConstraints, workingPermutation);

                // try to form blocks

                iterationBlockTranslatedInfo = CausalSets.BlockResponsibility.translateIntoBlocks(reorderedWorkingPermutation, workingConstraints);

                newBlocksWereFormed = iterationBlockTranslatedInfo.getElementsOfBlock().Count != reorderedWorkingPermutation.Count;
                if( !newBlocksWereFormed )
                {
                    break;
                }

                // if new blocks were formed we are here

                translateInfoStack.Add(iterationBlockTranslatedInfo);

                workingPermutation = iterationBlockTranslatedInfo.getElementsOfBlock();
                workingConstraints = iterationBlockTranslatedInfo.getConstraintsOfBlock();
            }

            // then we reconstruct the blocks from the block infos on the stack
            for(;;)
            {
                CausalSets.BlockResponsibility.TranslateInfo popedTranslateInfo;
                List<int> expandedPermutation;

                if( translateInfoStack.Count == 0 )
                {
                    break;
                }

                // pop
                popedTranslateInfo = translateInfoStack[translateInfoStack.Count - 1];
                translateInfoStack.RemoveAt(translateInfoStack.Count - 1);

                expandedPermutation = new List<int>();
                CausalSets.BlockResponsibility.translateFromBlocks(popedTranslateInfo, workingPermutation, expandedPermutation);

                workingPermutation = expandedPermutation;

            }
            
            // check if the result violates the orginal constraints
            checkInvariantOfPermutation(workingPermutation, inputConstraints);

            return workingPermutation;
        }

        static private void checkInvariantOfPermutation(List<int> permutation, List<CausalSets.Constraint> constraints)
        {
            if(true)
            {
                System.Diagnostics.Debug.Assert(!violatedConstraints(permutation, constraints), "Invalid permutation");
            }
        }


        static private bool violatedConstraints(List<int> permutation, List<CausalSets.Constraint> constraints)
        {
            foreach( CausalSets.Constraint iterationConstraint in constraints )
            {
                int preiorIndex;
                int posteriorIndex;

                // search for the index of the preior
                for (preiorIndex = 0; preiorIndex < permutation.Count; preiorIndex++)
                {
                    if (permutation[preiorIndex] == iterationConstraint.preiorElement)
                    {
                        break;
                    }
                }

                bool orderIsCorrect;

                orderIsCorrect = false;

                for (posteriorIndex = preiorIndex + 1; posteriorIndex < permutation.Count; posteriorIndex++)
                {
                    if (permutation[posteriorIndex] == iterationConstraint.postierElement)
                    {
                        orderIsCorrect = true;
                        break;
                    }
                }

                if (!orderIsCorrect)
                {
                    return true;
                }
            }

            return false;
        }

        // is the algorithm used for finding the permutation which minimizes the energy
        // must be constructed and initialized from outside
        public PartialOrderedSet.NetworkAlgorithm2 causalNeuronBasedAlgorithm;
    }
}
