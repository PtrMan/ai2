using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CausalSets
{
    /**
     * 
     * used to identify and create blocks
     * can be used to reverse a blocking operation
     * 
     * The block grouping is aggressive, which means that not only "single linked blocks" are grouped to blocks
     * (dependencies can link into the middle elements of an block)
     * 
     */
    class BlockResponsibility
    {
        public class TranslateInfo
        {
            public class BlockInfo
            {
                // can also contain only one element
                public List<int> orginalElements = new List<int>();

                // the dependencies of this block
                // is only in one way because in this way a translation is fast
                public List<int> dependencyBlockIndices = new List<int>();
            }

            public List<BlockInfo> blocks = new List<BlockInfo>();

            // this is in this class because it fits better in here than in the Responsibility  itself
            public List<CausalSets.Constraint> getConstraintsOfBlock()
            {
                List<CausalSets.Constraint> resultList;
                int blockIndex;

                resultList = new List<CausalSets.Constraint>();

                for( blockIndex = 0; blockIndex < blocks.Count; blockIndex++ )
                {
                    foreach( int iterationDependencyBlockIndex in blocks[blockIndex].dependencyBlockIndices )
                    {
                        CausalSets.Constraint newConstraint;

                        newConstraint = new CausalSets.Constraint();
                        newConstraint.preiorElement = iterationDependencyBlockIndex;
                        newConstraint.postierElement = blockIndex;

                        resultList.Add(newConstraint);
                    }
                }

                return resultList;
            }

            // get elements of block
            // (is a simple function which enumerates all block indices)
            public List<int> getElementsOfBlock()
            {
                List<int> resultList;
                int i;

                resultList = new List<int>();

                for( i = 0; i < blocks.Count; i++ )
                {
                    resultList.Add(i);
                }

                return resultList;
            }
            
            //public List<PartialOrderedSet.NetworkAlgorithm2.Constraint> orginalConstraints;
        }

        private class ElementInfo
        {
            // -1 means there is no block id associated with the element
            public int blockId = -1;

            public List<int> dependencies = new List<int>();
        }


        /**
         * tries to group the chained elements into blocks
         * 
         * the elements must be in a valid order allready
         * 
         * this function also translates all constraints into the blocks of TranslateInfo
         * 
         */
        static public TranslateInfo translateIntoBlocks(List<int> elements, List<CausalSets.Constraint> constraints)
        {
            int i;
            int elementI;
            int blockIdCounter;
            bool blockIdCounterWasIncremented;
            TranslateInfo resultTranslateInfo;
            
            ElementInfo[] elementInfos;

            debugInput(elements, constraints);

            resultTranslateInfo = new TranslateInfo();

            elementInfos = new ElementInfo[elements.Count];

            for( i = 0; i < elements.Count; i++ )
            {
                elementInfos[i] = new ElementInfo();
            }

            // go througth all the constraints and search the index of the dependencies and the element then store it in the array
            storeDependenciesIntoElementInfos(elements, elementInfos, constraints);
            

            // search in the array for interlinked blocks, label the elements with a blockcounter
            blockIdCounter = 0;
            blockIdCounterWasIncremented = false;

            for( elementI = 1; elementI < elements.Count; elementI++ )
            {
                if( elementInfos[elementI].dependencies.Count > 0 && elementInfos[elementI].dependencies.Contains(elementI-1) )
                {
                    elementInfos[elementI].blockId = blockIdCounter;
                }
                else
                {
                    blockIdCounter++;
                    elementInfos[elementI].blockId = blockIdCounter;
                }
            }

            // store the elements into the blocks
            // and transfer the orginal constraints to the blocks
            storeElementsIntoBlocks(elements, elementInfos, resultTranslateInfo);
            checkInvariantForBlocks(elements, resultTranslateInfo);
            transferConstraintsToBlocks(resultTranslateInfo, constraints);

            debugResultTranslateInfo(resultTranslateInfo);

            return resultTranslateInfo;
        }

        /**
         * \param blockInfo the blockInfo that should be translated
         * \param permutation the input permutation
         * \param outputElements will contain the orginal elements
         * \param outputConstraints will contain the orginal constraints
         * 
         */
        static public void translateFromBlocks(TranslateInfo blockInfo, List<int> permutation, List<int> outputElements/*, List<PartialOrderedSet.NetworkAlgorithm2.Constraint> outputConstraints*/)
        {
            outputElements.Clear();

            foreach( int blockIndex in permutation )
            {
                outputElements.AddRange(blockInfo.blocks[blockIndex].orginalElements);
            }
        }

        /**
         * 
         * stores the elements into the coresponding blocks
         * a block can contain at least one element (from the orginal)
         * 
         */
        static private void storeElementsIntoBlocks(List<int> elements, ElementInfo[] elementInfos, TranslateInfo resultTranslateInfo)
        {
            // go element for element and handle blocks like elements
            int elementI;
            int lastBlockId;

            lastBlockId = -1;

            for( elementI = 0; elementI < elements.Count; elementI++ )
            {
                if( elementInfos[elementI].blockId != -1 )
                {
                    // NOTE< element can't be at the beginning, so we don't need to test for this boundary condition that this is the first element >

                    if( elementInfos[elementI].blockId != lastBlockId )
                    {
                        TranslateInfo.BlockInfo newBlockInfo;

                        lastBlockId = elementInfos[elementI].blockId;

                        // add new block

                        newBlockInfo = new TranslateInfo.BlockInfo();
                        newBlockInfo.orginalElements.Add(elements[elementI]);

                        resultTranslateInfo.blocks.Add(newBlockInfo);
                    }
                    else
                    {
                        // add to last block
                        resultTranslateInfo.blocks[resultTranslateInfo.blocks.Count - 1].orginalElements.Add(elements[elementI]);
                    }
                }
                else
                {
                    TranslateInfo.BlockInfo newBlockInfo;

                    // reset because we are no more in an block
                    lastBlockId = -1;

                    newBlockInfo = new TranslateInfo.BlockInfo();
                    newBlockInfo.orginalElements.Add(elements[elementI]);

                    resultTranslateInfo.blocks.Add(newBlockInfo);
                }
            }
        }


        /**
         * links the blocks together after the old linkage information if the orginal elements
         * 
         * 
         */
        static private void transferConstraintsToBlocks(TranslateInfo resultTranslateInfo, List<CausalSets.Constraint> constraints)
        {
            foreach( CausalSets.Constraint iterationConstraint in constraints )
            {
                int posteriorBlockIndex;
                int posteriorBlockElementIndex;

                int preteriorBlockIndex;
                int preteriorBlockElementIndex;

                // for debugging
                bool preteriorBlockFound;
                bool posteriorBlockFound;

                preteriorBlockFound = false;
                posteriorBlockFound = false;

                // search block where the posterior is contained
                for( posteriorBlockIndex = 0; posteriorBlockIndex < resultTranslateInfo.blocks.Count; posteriorBlockIndex++ )
                {
                    bool found;

                    found = false;

                    for( posteriorBlockElementIndex = 0; posteriorBlockElementIndex < resultTranslateInfo.blocks[posteriorBlockIndex].orginalElements.Count; posteriorBlockElementIndex++ )
                    {
                        int elementInBlock;

                        elementInBlock = resultTranslateInfo.blocks[posteriorBlockIndex].orginalElements[posteriorBlockElementIndex];

                        if( elementInBlock == iterationConstraint.postierElement )
                        {
                            found = true;
                            posteriorBlockFound = true;
                            break;
                        }
                    }

                    if( found )
                    {
                        break;
                    }
                }

                System.Diagnostics.Debug.Assert(posteriorBlockFound);

                // the same for the preterior
                for (preteriorBlockIndex = 0; preteriorBlockIndex < resultTranslateInfo.blocks.Count; preteriorBlockIndex++)
                {
                    bool found;

                    found = false;

                    for (preteriorBlockElementIndex = 0; preteriorBlockElementIndex < resultTranslateInfo.blocks[preteriorBlockIndex].orginalElements.Count; preteriorBlockElementIndex++)
                    {
                        int elementInBlock;

                        elementInBlock = resultTranslateInfo.blocks[preteriorBlockIndex].orginalElements[preteriorBlockElementIndex];

                        if (elementInBlock == iterationConstraint.preiorElement )
                        {
                            found = true;
                            preteriorBlockFound = true;
                            break;
                        }
                    }

                    if( found )
                    {
                        break;
                    }
                }

                System.Diagnostics.Debug.Assert(preteriorBlockFound);

                if( posteriorBlockIndex != preteriorBlockIndex )
                {
                    resultTranslateInfo.blocks[posteriorBlockIndex].dependencyBlockIndices.Add(preteriorBlockIndex);
                }
            }
        }


        static private void storeDependenciesIntoElementInfos(List<int> elements, ElementInfo[] elementInfos, List<CausalSets.Constraint> constraints)
        {
            foreach( CausalSets.Constraint iterationConstraint in constraints )
            {
                int preiorElementIndex;
                int postiorElementIndex;

                // actually for debugging/problem tracing
                bool preiorFound;
                bool posteriorFound;

                preiorFound = false;
                posteriorFound = false;

                for( preiorElementIndex = 0; preiorElementIndex < elements.Count; preiorElementIndex++ )
                {
                    if( elements[preiorElementIndex] == iterationConstraint.preiorElement )
                    {
                        preiorFound = true;
                        break;
                    }
                }

                System.Diagnostics.Debug.Assert(preiorFound);

                for (postiorElementIndex = preiorElementIndex+1; postiorElementIndex < elements.Count; postiorElementIndex++)
                {
                    if (elements[postiorElementIndex] == iterationConstraint.postierElement)
                    {
                        posteriorFound = true;
                        break;
                    }
                }


                System.Diagnostics.Debug.Assert(posteriorFound);

                elementInfos[postiorElementIndex].dependencies.Add(preiorElementIndex);
            }
        }

        /** \brief checks the invariant, that the order of the elements must be preserved in the result
         *
         */
        static private void checkInvariantForBlocks(List<int> elements, TranslateInfo resultTranslateInfo)
        {
            if(true)
            {
                int elementI;
                int blockIndex;
                int indexOfElementInBlock;

                blockIndex = 0;
                indexOfElementInBlock = 0;

                for( elementI = 0; elementI < elements.Count; elementI++ )
                {
                    int orginalElement;
                    int translatedElement;

                    orginalElement = elements[elementI];

                    System.Diagnostics.Debug.Assert(resultTranslateInfo.blocks.Count > blockIndex, "Too few blocks!");

                    translatedElement = resultTranslateInfo.blocks[blockIndex].orginalElements[indexOfElementInBlock];
                    
                    indexOfElementInBlock++;
                    if( indexOfElementInBlock >= resultTranslateInfo.blocks[blockIndex].orginalElements.Count )
                    {
                        blockIndex++;
                        indexOfElementInBlock = 0;
                    }

                    System.Diagnostics.Debug.Assert(orginalElement == translatedElement);
                }
            }

        }

        static private void debugInput(List<int> elements, List<CausalSets.Constraint> constraints)
        {
            if(false)
            {
                System.Console.WriteLine("BlockResponsibility Input");
                System.Console.WriteLine("  permutation");

                System.Console.Write("    ");

                foreach( int iterationElement in elements )
                {
                    System.Console.Write(iterationElement.ToString() + " ");
                }
                System.Console.Write("\n");

                System.Console.WriteLine("  constraints");

                foreach (CausalSets.Constraint iterationConstraint in constraints)
                {
                    System.Console.WriteLine("    " + iterationConstraint.preiorElement.ToString() + "->" + iterationConstraint.postierElement.ToString());
                }


                System.Console.WriteLine("=====");
            }
        }

        static private void debugResultTranslateInfo(TranslateInfo resultTranslateInfo)
        {
            if(false)
            {
                int blockCounter;
                
                System.Console.WriteLine("BlockResponsibility Result");

                System.Console.WriteLine("  blocks");

                blockCounter = 0;

                foreach( TranslateInfo.BlockInfo iterationBlock in resultTranslateInfo.blocks )
                {
                    System.Console.WriteLine("      ({0})", blockCounter);
                    System.Console.WriteLine("      orginal elements");

                    foreach( int element in iterationBlock.orginalElements )
                    {
                        System.Console.Write("{0} ", element);
                    }
                    System.Console.WriteLine();

                    System.Console.WriteLine("      block dependencies");
                    foreach( int dependency in iterationBlock.dependencyBlockIndices )
                    {
                        System.Console.Write("{0} ", dependency);
                    }
                    System.Console.WriteLine("");

                    System.Console.WriteLine("");

                    blockCounter++;
                }

                System.Console.WriteLine("=====");
            }
        }
    }
}
