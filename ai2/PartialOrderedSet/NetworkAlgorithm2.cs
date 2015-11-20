using System;
using System.Collections.Generic;

using System.IO;

// TODO< test rotation >
// TODO< overhaul tunnel logic >
// TODO< implement families >

namespace PartialOrderedSet
{
    // another try for an netowrk algorithm to solve a causal set

    // is the scope constriction algorithm (SCA) algorithm as described in "Reasoning with Computer Code: a new Mathematical Logic" (Chapter 4)

    // the elements point at the neurons where they are mapped

    class NetworkAlgorithm2
    {
        private enum EnumCommuteAction
        {
            COMMUTEADJACENTELEMENTS,
            COMMUTEDISTANTELEMENTS
        }

        // a neuron doesn't move because it is not efficient
        private class Neuron
        {
            // these there variables are constant over the runtime of the iterations

            public int lengthOfPredecessors;
            public int lengthOfSuccessors;

            public int lengthDifference; // lengthOfPredecessors - lengthOfSuccessors
                                         // is in the paper described as the "net pull"

            // list of indices of the Neurons which must be before this neuron
            // OLD public List<int> precedenceNeuronIndices = new List<int>();

            public void calculateNumberDifference()
            {
                lengthDifference = lengthOfPredecessors - lengthOfSuccessors;
            }
        }

        private class Element
        {
            public List<int> beforeElementIndices = new List<int>();
            public List<int> afterElementIndices = new List<int>();

            public List<int> neuronIndices = new List<int>();

            public Element clone()
            {
                Element cloned;

                cloned = new Element();

                foreach( int iterationElement in beforeElementIndices )
                {
                    cloned.beforeElementIndices.Add(iterationElement);
                }

                foreach (int iterationElement in afterElementIndices)
                {
                    cloned.afterElementIndices.Add(iterationElement);
                }

                foreach( int neuronIndex in neuronIndices )
                {
                    cloned.neuronIndices.Add(neuronIndex);
                }

                return cloned;
            }
        }

        /** \breif used to store information abot a block of elements
         * 
         * 
         */
        private class Block
        {
            public int startIndex = -1; // index in elements of the start
            public int length = 0; // length of the block

            public Block clone()
            {
                Block cloned;

                cloned = new Block();
                cloned.startIndex = startIndex;
                cloned.length = length;

                return cloned;
            }
        }

        private class PoolElement
        {
            public PoolElement(int elementCount)
            {
                int i;
                
                elements = new List<Element>();

                for( i = 0; i < elementCount; i++ )
                {
                    elements.Add(new Element());
                }
            }

            public List<Element> elements;

            public List<Block> blocks = new List<Block>();
        }

        private class Family
        {
            public List<PoolElement> pool = new List<PoolElement>();
        }

        public List<int> doWork(List<CausalSets.Constraint> constraints, List<int> inputPermutation)
        {
            int currentIteration;
            int lastBetterIteration;

            int messageCounter = 0;

            lastBetterIteration = 0;

            poolAllreadyExpanded = false;

            initializeElementAndNeurons(constraints, inputPermutation);

            poolExpand(poolInitialSize);

            debugElements(pool[0].elements, inputPermutation);
            
            for( currentIteration = 0; currentIteration < maxIterations; currentIteration++ )
            {
                bool functionalDecreased;

                if( triesUntilGiveUp != -1 && lastBetterIteration + triesUntilGiveUp < currentIteration )
                {
                    if( poolAllreadyExpanded )
                    {
                        break;
                    }
                    else
                    {

                        /*
                        bool tunneled;

                        tunnelElements(out tunneled);
                        if( tunneled )
                        {
                            System.Console.WriteLine("tunneled at least one element, continue search");

                            lastBetterIteration = currentIteration;

                            continue;
                        }
                         */
                        
                        System.Console.WriteLine("expand pool");

                        
                        checkInvariantOfElementsInPool(0);

                        poolExpand(poolMaxSize);
                        poolAllreadyExpanded = true;

                        for( int poolIndex = 0; poolIndex < pool.Count; poolIndex++ )
                        {
                            checkInvariantOfElementsInPool(poolIndex);
                        }
                            

                        lastBetterIteration = currentIteration;
                    }
                }

                iterate(currentIteration, out functionalDecreased);
                if( functionalDecreased )
                {
                    Console.WriteLine("functional decreased, new permutation");
                    debugElements(pool[0].elements, inputPermutation);

                    messageCounter++;

                    if( (messageCounter % 1) == 0 )
                        Console.WriteLine("functional decreased {0} poolsize {1} length of [0] is {3}, functional of [0] is {2}", currentIteration, pool.Count, calculateFunctional(pool[0].elements, inputPermutation, constraints), pool[0].elements.Count);

                    lastBetterIteration = currentIteration;
                }
            }

            Console.WriteLine("result functional is {0}", calculateFunctional(pool[0].elements, inputPermutation, constraints));




            
            // we assume that the functional of the first element is equal to all other pool items
            return reconstructResult(pool[0].elements, inputPermutation);
        }

        private void iterate(int currentIteration, out bool functionalDecreased)
        {
            int poolIndex;
            
            bool canCommuteVariable;

            int differentialPull;


            //int adjacedentElementIndex;
            int elementIndexA;
            int elementIndexB;

            functionalDecreased = false;

            for( poolIndex = 0; poolIndex < pool.Count; poolIndex++ )
            {
                int currentIterationModulo;
                int tunnelIterationModulo;

                EnumCommuteAction commuteAction;

                currentIterationModulo = currentIteration % 2; // DEBUGHACK
                tunnelIterationModulo = (currentIteration + 1) % 10;

                if( tunnelIterationModulo == 0 )
                {
                    int tunnelEndIndex;
                    bool canTunnelToFront;

                    tunnelEndIndex = random.Next(pool[poolIndex].elements.Count - 1);

                    canTunnelToFront = canTunnelToFrontAndLowersFunctional(poolIndex, tunnelEndIndex);



                    if( canTunnelToFront )
                    {
                        // TODO< check if allready a family with that index exist >

                        System.Console.WriteLine("tunnel end index {0}", tunnelEndIndex);

                        checkInvariantOfElementsInPool(poolIndex);
                        rotateRight(pool[poolIndex].elements, 0, tunnelEndIndex);
                        checkInvariantOfElementsInPool(poolIndex);

                        // TODO< set another variable because we haven't really decremented the functional >
                        functionalDecreased = true;
                    }
                }

                if( currentIterationModulo == 0 )
                {
                    // try to commute directly adjacent pairs

                    // choose two adjacent elements
                    // -1 because we need one element to the right that we compare
                    elementIndexA = random.Next(pool[poolIndex].elements.Count - 1 - 1);
                    elementIndexB = elementIndexA + 1;

                    commuteAction = EnumCommuteAction.COMMUTEADJACENTELEMENTS;
                }
                else
                {
                    if( pool[poolIndex].elements.Count <= 2 )
                    {
                        continue;
                    }

                    // try to commute any pairs

                    elementIndexA = random.Next(pool[poolIndex].elements.Count-1);

                    for (; ; )
                    {
                        elementIndexB = random.Next(pool[poolIndex].elements.Count-1);

                        if (elementIndexA != elementIndexB)
                        {
                            break;
                        }
                    }

                    // sort so elementA is allways before elementB
                    if( elementIndexA > elementIndexB )
                    {
                        int tempElementIndex;

                        tempElementIndex = elementIndexA;
                        elementIndexA = elementIndexB;
                        elementIndexB = tempElementIndex;
                    }

                    commuteAction = EnumCommuteAction.COMMUTEDISTANTELEMENTS;
                }


                

                if( commuteAction == EnumCommuteAction.COMMUTEADJACENTELEMENTS )
                {
                    canCommuteVariable = canCommuteAdjacentElements(pool[poolIndex].elements, elementIndexA, elementIndexB);

                    Console.WriteLine("can commute from i {0}? -> {1}", elementIndexA, canCommuteVariable);
                }
                else if( commuteAction == EnumCommuteAction.COMMUTEDISTANTELEMENTS )
                {
                    canCommuteVariable = canCommuteDistantElements(pool[poolIndex].elements, elementIndexA, elementIndexB);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false, "commuteAction is invalid!");

                    // to make the compiler happy
                    canCommuteVariable = false;
                }

                if (!canCommuteVariable)
                {
                    continue;
                }

                // we are here if they could be commute/swaped

                //neuronIndexA = pool[poolIndex].elements[elementIndexA].neuronIndex;
                //neuronIndexB = pool[poolIndex].elements[elementIndexB].neuronIndex;

                // calculate the "differential pull"

                // NOTE< we assume that the pull of a block is the sum of the pulls of the elements >
                differentialPull = calculateDifferentialPull(poolIndex, elementIndexA, elementIndexB);

                Console.WriteLine("differential pull = {0}", differentialPull);

                if (
                    differentialPull < 0
                    
                )
                {
                    // we commute, store the elements, flush the pool, fill the pool and return

                    functionalDecreased = true;

                    checkInvariantOfElementsInPool(poolIndex);

                    exchange(poolIndex, elementIndexA, elementIndexB);

                    // we actually try to fuse the first element, but its twisted because it was allready exchanged
                    //tryToFuseWithNeightbors(poolIndex, elementIndexB);

                    checkInvariantOfElementsInPool(poolIndex);

                    if( pool.Count > 1 )
                    {
                        List<Element> elements;
                        List<Block> blocks;

                        elements = pool[poolIndex].elements;
                        blocks = pool[poolIndex].blocks;
                        flushPool();
                        populatePool(elements, blocks);
                    }
                    
                    return;
                }
                else if( differentialPull == 0 && settingCommuteOnZero )
                {
                    // we commute

                    checkInvariantOfElementsInPool(poolIndex);

                    exchange(poolIndex, elementIndexA, elementIndexB);

                    // we actually try to fuse the first element, but its twisted because it was allready exchanged
                    tryToFuseWithNeightbors(poolIndex, elementIndexB);

                    checkInvariantOfElementsInPool(poolIndex);
                }
            }
            
        }

        /**
         * 
         * tunnels elements as desribed in his email
         * 
         * tunneling of elements is putting the element in front of the "wall" to avoid local minima
         * 
         */
        /*
        private void tunnelElements(out bool tunneled)
        {

            int poolIndex;

            tunneled = false;

            for( poolIndex = 0; poolIndex < pool.Count; poolIndex++ )
            {
                PoolElement poolElement;
                int lastElementI;

                poolElement = pool[poolIndex];

                for( lastElementI = poolElement.elements.Count-1; lastElementI > 0; lastElementI-- )
                {
                    int firstElementI;

                    for( firstElementI = lastElementI-1; firstElementI > 0; firstElementI-- )
                    {
                        int differentialPull;
                        bool canCommute;

                        differentialPull = calculateDifferentialPull(poolIndex, firstElementI, lastElementI);
                        canCommute = canCommuteDistantElements(pool[poolIndex].elements, firstElementI, lastElementI);

                        if( differentialPull < 0 && canCommute )
                        {
                            int y = 0;

                            exchange(poolIndex, firstElementI, lastElementI);

                            tunneled = true;
                            break;
                        }
                    }
                }
            }

            for( poolIndex = 0; poolIndex < pool.Count; poolIndex++ )
            {
                PoolElement poolElement;

                poolElement = pool[poolIndex];

                // debug the elements wih the pull
                // into a file

                using (StreamWriter outfile = new StreamWriter("C:\\Users\\r0b3\\temp\\log58585.txt"))
                {
                    int elementI;

                    for( elementI = 0; elementI < poolElement.elements.Count-1; elementI++ )
                    {
                        outfile.WriteLine("-    " + elementI.ToString());
                        outfile.WriteLine("pull " + calculateDifferentialPull(poolIndex, elementI, elementI+1).ToString());


                        outfile.WriteLine();

                    }
                    
                }
            }

        }
         * */

        private int calculateDifferentialPull(int poolIndex, int elementIndexA, int elementIndexB)
        {
            int pullPositive;
            int pullNegative;
            int differentialPull;

            pullPositive = 0;
            System.Diagnostics.Debug.Assert(pool[poolIndex].elements[elementIndexA].neuronIndices.Count == 1);

            pullPositive += neurons[pool[poolIndex].elements[elementIndexA].neuronIndices[0]].lengthDifference;


            pullNegative = 0;
            System.Diagnostics.Debug.Assert(pool[poolIndex].elements[elementIndexB].neuronIndices.Count == 1);


            pullNegative += neurons[pool[poolIndex].elements[elementIndexB].neuronIndices[0]].lengthDifference;

            System.Console.WriteLine("pull + {0}  - {1}", pullPositive, pullNegative);

            differentialPull = pullPositive - pullNegative;

            return differentialPull;
        }

        /**
         * tries to fuse elements aggresivly (does fuse even when other elements point into the middle or the two elements point from the middle at something)
         * 
         * 
         * 
         */
        private void tryToFuseWithNeightbors(int poolIndex, int elementIndex)
        {
            PoolElement poolElement;
            bool tryToFuseWithBegin;
            bool tryToFuseWithEnd;

            poolElement = pool[poolIndex];

            tryToFuseWithBegin = elementIndex != 0;
            tryToFuseWithEnd = elementIndex != poolElement.elements.Count - 1;

            if( tryToFuseWithBegin )
            {
                if(
                    (poolElement.elements[elementIndex].beforeElementIndices.Contains(elementIndex - 1)) &&
                    (poolElement.elements[elementIndex-1].afterElementIndices.Contains(elementIndex))
                )
                {
                    checkInvariantOfElementsInPool(poolIndex);

                    // remove self linkage
                    poolElement.elements[elementIndex - 1].afterElementIndices.Remove(elementIndex);
                    poolElement.elements[elementIndex].beforeElementIndices.Remove(elementIndex + 1);

                    // this is needed because else it would point nowhere
                    poolElement.elements[elementIndex - 1].afterElementIndices.AddRange(poolElement.elements[elementIndex].afterElementIndices);
                    poolElement.elements[elementIndex - 1].beforeElementIndices.AddRange(poolElement.elements[elementIndex].beforeElementIndices);

                    // TODO< rewire outside linkage >



                    // to fuse we need to append our neurons to the element before us
                    poolElement.elements[elementIndex - 1].neuronIndices.AddRange(poolElement.elements[elementIndex].neuronIndices);

                    
                    

                    removeElementAtAndRelink(poolIndex, elementIndex);

                    checkInvariantOfElementsInPool(poolIndex);

                    // elementIndex is invalid
                    elementIndex = -1;

                    return;
                }
            }

            // TODO< try to fuse with end >
        }

        private void removeElementAtAndRelink(int poolIndex, int elementIndex)
        {
            PoolElement poolElement;
            int elementI;

            poolElement = pool[poolIndex];

            // first we relink everything

            for( elementI = 0; elementI < elementIndex; elementI++ )
            {
                int afterI;
                
                for( afterI = 0; afterI < poolElement.elements[elementI].afterElementIndices.Count; afterI++ )
                {
                    if( poolElement.elements[elementI].afterElementIndices[afterI] > elementIndex )
                    {
                        poolElement.elements[elementI].afterElementIndices[afterI]--;
                    }
                }
            }

            for( elementI = elementIndex+1; elementI < poolElement.elements.Count; elementI++ )
            {
                int beforeI;

                for( beforeI = 0; beforeI < poolElement.elements[elementI].beforeElementIndices.Count; beforeI++ )
                {
                    if( poolElement.elements[elementI].beforeElementIndices[beforeI] > elementIndex )
                    {
                        poolElement.elements[elementI].beforeElementIndices[beforeI]--;
                    }
                }
            }

            // then we remove the element
            poolElement.elements.RemoveAt(elementI);

            Console.WriteLine("remove");
        }

        private void exchange(int poolIndex, int elementIndexA, int elementIndexB)
        {
            PoolElement currentPool;
            Element tempElement;

            currentPool = pool[poolIndex];

            // rewire the linking
            // see "causalSets relink.png"

            foreach( int iterationAfterElementIndex in currentPool.elements[elementIndexB].afterElementIndices )
            {
                replaceIntInList(currentPool.elements[iterationAfterElementIndex].beforeElementIndices, elementIndexB, elementIndexA);
            }

            foreach( int iterationAfterElementIndex in currentPool.elements[elementIndexB].beforeElementIndices )
            {
                replaceIntInList(currentPool.elements[iterationAfterElementIndex].afterElementIndices, elementIndexB, elementIndexA);
            }

            foreach( int iterationAfterElementIndex in currentPool.elements[elementIndexA].afterElementIndices )
            {
                replaceIntInList(currentPool.elements[iterationAfterElementIndex].beforeElementIndices, elementIndexA, elementIndexB);
            }

            foreach( int iterationAfterElementIndex in currentPool.elements[elementIndexA].beforeElementIndices )
            {
                replaceIntInList(currentPool.elements[iterationAfterElementIndex].afterElementIndices, elementIndexA, elementIndexB);
            }

            // exchange
            tempElement = currentPool.elements[elementIndexA];
            currentPool.elements[elementIndexA] = currentPool.elements[elementIndexB];
            currentPool.elements[elementIndexB] = tempElement;
        }

        // helper
        static private void replaceIntInList(List<int> list, int orginalValue, int newValue)
        {
            int i;

            for( i = 0; i < list.Count; i++ )
            {
                if( list[i] == orginalValue )
                {
                    list[i] = newValue;
                    return;
                }
            }

            // should never be reached
            System.Diagnostics.Debug.Assert(false, "searched element was not found");
        }

        private void populatePool(List<Element> elements, List<Block> blocks)
        {
            int i;
            int poolsize;

            if( poolAllreadyExpanded )
            {
                poolsize = poolMaxSize;
            }
            else
            {
                poolsize = poolInitialSize;
            }

            for( i = 0; i < poolsize; i++ )
            {
                int elementI;

                pool.Add(new PoolElement(neurons.Length));


                // clone all elements
                for (elementI = 0; elementI < elements.Count; elementI++)
                {
                    pool[i].elements[elementI] = elements[elementI].clone();
                }

                foreach( Block iterationBlock in blocks )
                {
                    pool[i].blocks.Add(iterationBlock.clone());
                }
            }
        }

        private void poolExpand(int newPoolSize)
        {
            int i;
            
            
            for (i = pool.Count; i < newPoolSize; i++)
            {
                int elementI;
                
                pool.Add(new PoolElement(neurons.Length));

                System.Diagnostics.Debug.Assert(pool[0].elements.Count != 0);

                // clone all elements
                for( elementI = 0; elementI < pool[0].elements.Count; elementI++ )
                {
                    pool[i].elements[elementI] = pool[0].elements[elementI].clone();
                }

                foreach (Block iterationBlock in pool[0].blocks)
                {
                    pool[i].blocks.Add(iterationBlock.clone());
                }
            }
        }

        private bool canCommuteAdjacentElements(List<Element> parameterElements, int elementIndexA, int elementIndexB)
        {
            // check if the neuronIndexBefore appears in the precedence list of the after neuron
            // if this is the case the two elements can't be interchanged
            return !parameterElements[elementIndexB].beforeElementIndices.Contains(elementIndexA);
        }

        private bool canCommuteDistantElements(List<Element> parameterElements, int elementIndexA, int elementIndexB)
        {
            if (!canCommuteAdjacentElements(parameterElements, elementIndexA, elementIndexB))
            {
                return false;
            }

            // check that no dependencies are for elementB between A and B
            foreach( int iterationDependentElementIndex in parameterElements[elementIndexB].beforeElementIndices )
            {
                if( iterationDependentElementIndex >= elementIndexA )
                {
                    return false;
                }
            }

            // same for elementA
            foreach (int iterationDependentElementIndex in parameterElements[elementIndexA].afterElementIndices)
            {
                if (iterationDependentElementIndex <= elementIndexB)
                {
                    return false;
                }
            }


            return true;
        }

        private void initializeElementAndNeurons(List<CausalSets.Constraint> constraints, List<int> inputPermutation)
        {
            int i;
            int neuronI;
            
            // add one PoolElement to the pool
            flushPool();
            pool.Add(new PoolElement(inputPermutation.Count));

            neurons = new Neuron[inputPermutation.Count];

            // allocate
            for( i = 0; i < inputPermutation.Count; i++ )
            {
                neurons[i] = new Neuron();
            }

            // initialize
            for( i = 0; i < inputPermutation.Count; i++ )
            {
                // set it at the index because it is the initialized order
                pool[0].elements[i].neuronIndices.Add(i);
            }

            // because the neuron indices are equal to the element indices we can now
            // * for each neuron
            //  * for each constraint
            //   - if the constraint posterior element is the inputPermutation element at the current index
            //     - search the index of the constraint preteroir element
            //     - add the index to the "beforeElementIndices" list of the neuron
            //     - add the reversed direction to the preterior element
            //     
            //     - additionally we add one to the "numberOfSuccessors" of the neuron indexed by index
            //       this reduces the overhead to go again through the list and do this

            for( neuronI = 0; neuronI < inputPermutation.Count; neuronI++ )
            {
                foreach( CausalSets.Constraint iterationConstraint in constraints )
                {
                    if( iterationConstraint.postierElement == inputPermutation[neuronI] )
                    {
                        int preteroirIndex;
                        
                        // search the index of the constraint preterior element
                        for( preteroirIndex = 0; preteroirIndex < inputPermutation.Count; preteroirIndex++ )
                        {
                            if( inputPermutation[preteroirIndex] == iterationConstraint.preiorElement )
                            {
                                break;
                            }
                        }

                        // OLD neurons[neuronI].precedenceNeuronIndices.Add(preteroirIndex);
                        pool[0].elements[neuronI].beforeElementIndices.Add(preteroirIndex);
                        pool[0].elements[preteroirIndex].afterElementIndices.Add(neuronI);


                        // add one to indexed neuron value "numberOfSuccessors"
                        neurons[preteroirIndex].lengthOfSuccessors++;
                    }
                }
            }


            // set the variable "numberOfPredecessors" of all neurons
            // it is the count of the precedence neurons because they "point" at this neuron
            for (neuronI = 0; neuronI < inputPermutation.Count; neuronI++)
            {
                // OLD neurons[neuronI].lengthOfPredecessors = neurons[neuronI].precedenceNeuronIndices.Count;
                neurons[neuronI].lengthOfPredecessors = pool[0].elements[neuronI].beforeElementIndices.Count;
            }

            // calculate the difference for each neuron
            for( neuronI = 0; neuronI < inputPermutation.Count; neuronI++ )
            {
                neurons[neuronI].calculateNumberDifference();
            }

            checkInvariantOfElementsInPool(0);
        }

        private List<int> reconstructResult(List<Element> elements, List<int> inputPermutation)
        {
            List<int> result;
            int elementI;

            result = new List<int>();

            for (elementI = 0; elementI < elements.Count; elementI++)
            {
                foreach( int iterationNeuronIndex in elements[elementI].neuronIndices )
                {
                    result.Add(inputPermutation[iterationNeuronIndex]);
                }
            }

            return result;
        }

        // calculates the energy / functional of the current permutation
        private int calculateFunctional(List<Element> elements, List<int> inputPermutation, List<CausalSets.Constraint> constraints)
        {
            List<int> currentPermutation;
            
            currentPermutation = reconstructResult(elements, inputPermutation);

            return CausalSets.CausalSet.calculateFunctional(currentPermutation, constraints);
        }

        private void flushPool()
        {
            pool.Clear();
        }

        private void checkInvariantOfElementsInPool(int poolIndex)
        {
            if(true)
            {
                PoolElement poolElement;
                int iterationElementI;

                poolElement = pool[poolIndex];

                for (iterationElementI = 0; iterationElementI < poolElement.elements.Count; iterationElementI++)
                {
                    foreach (int iterationAfterElementIndex in poolElement.elements[iterationElementI].afterElementIndices)
                    {
                        System.Diagnostics.Debug.Assert(iterationAfterElementIndex >= 0 && iterationAfterElementIndex < poolElement.elements.Count);

                        System.Diagnostics.Debug.Assert(poolElement.elements[iterationAfterElementIndex].beforeElementIndices.Contains(iterationElementI));
                    }

                    foreach (int iterationBeforeElementIndex in poolElement.elements[iterationElementI].beforeElementIndices)
                    {
                        System.Diagnostics.Debug.Assert(iterationBeforeElementIndex >= 0 && iterationBeforeElementIndex < poolElement.elements.Count);

                        System.Diagnostics.Debug.Assert(poolElement.elements[iterationBeforeElementIndex].afterElementIndices.Contains(iterationElementI));
                    }

                }
            }
        }

        // checks whenever the element can tunnel to the front
        private bool canTunnelToFrontAndLowersFunctional(int poolIndex, int elementIndex)
        {
            bool canTunnel;
            PoolElement poolElement;
            int lengthDifferenceOfFirstElement;
            int lengthDifferenceOfCandidate;

            // before and after the element at elementIndex which could be moved to the first position
            int lengthDifferenceOfElementBefore;
            int lengthDifferenceOfElementAfter;


            poolElement = pool[poolIndex];
            
            // the element index must point at the pre least element, because we need to compare the pull differences
            // to estimate if it is benificial
            System.Diagnostics.Debug.Assert(elementIndex < poolElement.elements.Count - 1);

            // a element can tunnel to the front when it doesn't have any dependencies
            canTunnel = poolElement.elements[elementIndex].beforeElementIndices.Count == 0;

            if( !canTunnel )
            {
                return false;
            }
            // we are here if it could tunnel to the beginning

            // now we need to estimate if it is beneficial

            lengthDifferenceOfFirstElement = neurons[poolElement.elements[0].neuronIndices[0]].lengthDifference;
            lengthDifferenceOfCandidate = neurons[poolElement.elements[elementIndex].neuronIndices[0]].lengthDifference;

            // it have to be
            //  lengthDifferenceOfCandidate < lengthDifferenceOfFirstElement
            // because only then it is beneficial
            if( lengthDifferenceOfCandidate >= lengthDifferenceOfFirstElement )
            {
                return false;
            }

            // now we make sure that the difference of the new border is at least 0

            lengthDifferenceOfElementBefore = neurons[poolElement.elements[elementIndex-1].neuronIndices[0]].lengthDifference;
            lengthDifferenceOfElementAfter = neurons[poolElement.elements[elementIndex+1].neuronIndices[0]].lengthDifference;

            if( lengthDifferenceOfElementBefore - lengthDifferenceOfElementAfter > 0 )
            {
                return false;
            }

            return true;
        }

        /**
         * 
         * elements are changed in place
         * the method manges the relinking
         * 
         */
        private static void rotateRight(List<Element> elements, int startIndex, int endIndex)
        {
            int i;
            int beforeElementIndicesI;
            int afterElementIndicesI;

            // only for startIndex = 0 is implemented
            System.Diagnostics.Debug.Assert(startIndex == 0);

            // relink all except last element
            for( i = startIndex; i < endIndex; i++ )
            {
                

                for( afterElementIndicesI = 0; afterElementIndicesI < elements[i].afterElementIndices.Count; afterElementIndicesI++ )
                {
                    int currentAfterElementIndex;

                    currentAfterElementIndex = elements[i].afterElementIndices[afterElementIndicesI];

                    //if( currentAfterElementIndex >= startIndex && currentAfterElementIndex < /* because last element is relinked seperatly */ endIndex )
                    {
                        // relink the linked before index adressed by after
                        replaceIntList(elements[currentAfterElementIndex].beforeElementIndices, i, i+1);

                        //elements[i].afterElementIndices[afterElementIndicesI];
                    }
                }

                
                for (beforeElementIndicesI = 0; beforeElementIndicesI < elements[i].beforeElementIndices.Count; beforeElementIndicesI++)
                {
                    int currentBeforeElementIndex;

                    currentBeforeElementIndex = elements[i].beforeElementIndices[beforeElementIndicesI];

                    //if (currentBeforeElementIndex >= startIndex && currentBeforeElementIndex < * because last element is relinked seperatly * endIndex)
                    {
                        // relink the linked before index adressed by after
                        replaceIntList(elements[currentBeforeElementIndex].afterElementIndices, i, i + 1);

                        //elements[i].beforeElementIndices[beforeElementIndicesI]++;
                    }
                }
            }

            // relink last element
            for( beforeElementIndicesI = 0; beforeElementIndicesI < elements[endIndex].beforeElementIndices.Count; beforeElementIndicesI++ )
            {
                int currentBeforeElementIndex;

                currentBeforeElementIndex = elements[endIndex].beforeElementIndices[beforeElementIndicesI];

                replaceIntInList(elements[currentBeforeElementIndex].afterElementIndices, endIndex, startIndex);
            }

            for( afterElementIndicesI = 0; afterElementIndicesI < elements[endIndex].afterElementIndices.Count; afterElementIndicesI++ )
            {
                int currentAfterElementIndex;

                currentAfterElementIndex = elements[endIndex].afterElementIndices[afterElementIndicesI];

                replaceIntInList(elements[currentAfterElementIndex].beforeElementIndices, endIndex, startIndex);
            }

            // rotate elements

            Element lastElement;

            lastElement = elements[endIndex];

            elements.RemoveAt(endIndex);
            elements.Insert(startIndex, lastElement);
        }


        private void debugElements(List<Element> elements, List<int> inputPermutation)
        {
            List<int> result;
            int elementI;

            string neuronDifferenceString = "";
            string indexString = "";
            string permutationString = "";

            result = new List<int>();

            for (elementI = 0; elementI < elements.Count; elementI++)
            {
                int neuronIndex;
                int neuronDifference;

                neuronIndex = elements[elementI].neuronIndices[0];
                
                neuronDifference = neurons[neuronIndex].lengthDifference;

                indexString += string.Format("[{0}]{1}", elementI, new String(' ', 7 - elementI.ToString().Length-2));
                neuronDifferenceString += string.Format("{0}{1}", neuronDifference, new String(' ', 7 - neuronDifference.ToString().Length));
                permutationString += string.Format("{0}{1}", inputPermutation[neuronIndex], new String(' ', 7- inputPermutation[neuronIndex].ToString().Length));
                
                
            }

            Console.WriteLine(indexString);
            Console.WriteLine(neuronDifferenceString);
            Console.WriteLine(permutationString);
            Console.WriteLine("---");
        }

        // a family is a collection with a common ancestor
        // this mechanism is used to climb over local minima even when the function could be higher
        // a new family is usually created with tunneling
        // a family has a own pool
        private void newFamily(List<Element> elements)
        {

        }

        // helper method
        // asserts if it was not found
        private static void replaceIntList(List<int> list, int oldValue, int newValue)
        {
            int foundIndex;

            foundIndex = list.IndexOf(oldValue);
            System.Diagnostics.Debug.Assert(foundIndex != -1);

            list[foundIndex] = newValue;
        }

        // must be set from outside
        public Random random;

        // public settable setting
        // if this is true we commute if the energy difference is zero
        public bool settingCommuteOnZero = false;

        // must be set from outside
        public uint maxIterations = 0;

        // are the iterations in that no change is occuring the algorithm is trying to get another permutation
        public int triesUntilGiveUp = -1;

        public int poolMaxSize = 1;
        public int poolInitialSize = 1;

        private List<PoolElement> pool = new List<PoolElement>();

        // we expand the pool when we didn't found any permutation with the poolsize of one
        // this sppeds up the algorithm greatly
        private bool poolAllreadyExpanded = false;

        private Neuron[] neurons;
    }
}
