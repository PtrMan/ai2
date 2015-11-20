using System;
using System.Collections.Generic;

using Datastructures;

// stochastic implementation of causal sets
// as described by
// http://www.scicontrols.com/
// (see papers)

// the input is a unordered causal set, the output is a ordered (suboptimal) causal set

// TODO< for nlp and other large sets we need a possibility to swap-out not needed/touched memory >
namespace PartialOrderedSet
{

    class PartialOrderedSetAlgorithm
    {
        public PartialOrderedSetAlgorithm(Random random)
        {
            this.random = random;
        }

        public void fillCausalRelations(uint nodesCount, List<Relation> relations)
        {
            int iNode;

            dag = new Dag<PartialOrderedSetDagElement>();
            rootElementIndices.Clear();

            for( iNode = 0; iNode < nodesCount; iNode++ )
            {
                dag.elements.Add(new Dag<PartialOrderedSetDagElement>.Element(new PartialOrderedSetDagElement()));
            }

            foreach( Relation iterationRelation in relations )
            {
                dag.elements[iterationRelation.sourceIndex].childIndices.Add(iterationRelation.destinationIndex);
            }

            // mark all referenced nodes as red
            // so all root nodes are unmarked
            foreach( Relation iterationRelation in relations )
            {
                dag.elements[iterationRelation.destinationIndex].content.isRed = true;
            }

            // search root nodes and add them
            iNode = 0;
            foreach( Dag<PartialOrderedSetDagElement>.Element iterationDagElement in dag.elements)
            {
                if( !iterationDagElement.content.isRed )
                {
                    rootElementIndices.Add(iNode);
                }

                iNode++;
            }
        }

        public void work(uint iterations)
        {
            uint currentIteration;

            initializeDag();

            for( currentIteration = 0; currentIteration < iterations; currentIteration++ )
            {
                int rating;
                List<int> permutation;

                permutation = generatePermutationWithRating(out rating);

                if( rating < bestRating )
                {
                    Console.WriteLine(rating);

                    bestPermutation = permutation;
                    bestRating = rating;
                }
            }
        }
        
        public List<int> getBestPermutation()
        {
            return bestPermutation;
        }

        // does count the reference Counters of the dag
        private void initializeDag()
        {
            foreach( Dag<PartialOrderedSetDagElement>.Element iterationDagElement in dag.elements )
            {
                iterationDagElement.content.resetReferenceCounter = 0;
            }

            foreach( Dag<PartialOrderedSetDagElement>.Element iterationDagElement in dag.elements )
            {
                foreach( int childIndex in iterationDagElement.childIndices )
                {
                    dag.elements[childIndex].content.resetReferenceCounter++;
                }
            }
        }

        private void resetDag()
        {
            foreach( Dag<PartialOrderedSetDagElement>.Element iterationDagElement in dag.elements )
            {
                iterationDagElement.content.reset();
            }
        }

        private List<int> generatePermutationWithRating(out int rating)
        {
            List<int> resultPermutation;

            resultPermutation = generatePermutationAndDecorateDag();
            rating = ratePermutation();

            return resultPermutation;
        }

        /** \brief rates the permutation
         * 
         */
        private int ratePermutation()
        {
            int resultRating;
            List<int> indexStack;

            resultRating = 0;

            // clone the list
            indexStack = new List<int>();
            foreach( int index in rootElementIndices )
            {
                indexStack.Add(index);
            }

            while( indexStack.Count > 0 )
            {
                int currentIndex;

                // ??? is this the equivalent of python pop
                currentIndex = indexStack[0];
                indexStack.RemoveAt(0);

                foreach( int iterationChildIndex in dag.elements[currentIndex].childIndices )
                {
                    resultRating += (dag.elements[iterationChildIndex].content.outputPosition - dag.elements[currentIndex].content.outputPosition - 1);
                }

                indexStack.AddRange(dag.elements[currentIndex].childIndices);
            }

            return resultRating;
        }

        /** \brief generates a permutation and decorates the Dag with the output positions of the elements in the permutation
	     *
         */
        private List<int> generatePermutationAndDecorateDag()
        {
            List<int> resultPermutation;
            List<int> headDagIndices;

            resultPermutation = new List<int>();

            resetDag();
            markRootsAsRed();

            // clone list
            headDagIndices = new List<int>();
            foreach( int index in rootElementIndices )
            {
                headDagIndices.Add(index);
            }

            for(;;)
            {
                int currentHeadIndexIndex, currentHeadIndex;

                // choose last head
                // works much better
                currentHeadIndexIndex = headDagIndices.Count - 1;
                currentHeadIndex = headDagIndices[currentHeadIndexIndex];
                
                // old strategy, choose random head
                //currentHeadIndexIndex = random.Next(headDagIndices.Count - 1);
                //currentHeadIndex = headDagIndices[currentHeadIndexIndex];

                // write index of head to output
                resultPermutation.Add(currentHeadIndex);
                // decorate chosen head with output position
                dag.elements[currentHeadIndex].content.outputPosition = resultPermutation.Count;

                // mark childs as red
                foreach( int iterationChildIndex in dag.elements[currentHeadIndex].childIndices )
                {
                    dag.elements[iterationChildIndex].content.isRed = true;
                }

                // decrement parent counters of childs
                foreach( int iterationChildIndex in dag.elements[currentHeadIndex].childIndices )
                {
                    // assert dag.elements[iterationChildIndex].content.referenceCounter > 0
                    dag.elements[iterationChildIndex].content.referenceCounter--;

                    // add child to heads if counter == 0
                    if( dag.elements[iterationChildIndex].content.referenceCounter == 0 )
                    {
                        headDagIndices.Add(iterationChildIndex);
                    }
                }

                // remove head
                headDagIndices.RemoveAt(currentHeadIndexIndex);

                // loop until no heads are there
                if( headDagIndices.Count == 0 )
                {
                    break;
                }
            }

            return resultPermutation;
        }

        private void markRootsAsRed()
        {
            foreach( int iterationRootIndex in rootElementIndices )
            {
                dag.elements[iterationRootIndex].content.isRed = true;
            }
        }

        private Dag<PartialOrderedSetDagElement> dag;
        private List<int> rootElementIndices = new List<int>();

        private List<int> bestPermutation;
        private int bestRating = int.MaxValue;
        private Random random;
    }
}
