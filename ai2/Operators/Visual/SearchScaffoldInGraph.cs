using System;
using System.Collections.Generic;

namespace Operators.Visual
{
    /**
     * this Operator takes a graph and tries to apply the GraphScaffold on it
     * the operator is tried non-exhausvly (each egde is used maximal once or not if its not possible)
     * 
     * the vertices of the graph must have following structure
     * 
     * vertex
     * [0] not used
     * [1] not used
     * [2] (vector2<float>) the middle/average position
     * 
     * this is compatible with the output from the "GroupPointsGrid" operator and the "ConnectGroups" operator
     * 
     * parameters
     * [0] graph
     * 
     * 
     * 
     */
    class SearchScaffoldInGraph : GeneticProgramming.TypeRestrictedOperator
    {
        public override void call(List<Datastructures.Variadic> parameters, List<Datastructures.Variadic> globals, out Datastructures.Variadic result, out GeneticProgramming.TypeRestrictedOperator.EnumResult resultCode)
        {
            Datastructures.TreeNode graphTreeNode;
            int edgesCount;
            int i;

            // list for the indices of the edges which aren't covered by the scaffold
            List<int> remainingEdgesIndices = new List<int>();

            bool scaffoldAppliedSuccessfull;
            List<int> scaffoldEdgesIndices;

            int tryCounter;
            const int MAXTRIES = 100;

            resultCode = EnumResult.FAILED;
            result = null;

            // BREAK this breaks genericity
            lines.Clear();

            if( parameters.Count != 1 )
            {
                return;
            }

            graphTreeNode = parameters[0].valueTree;

            edgesCount = graphTreeNode.childNodes[GRAPHINDEXEDGES].childNodes.Count;

            // put all edges into the remainingEdgesIndices
            for (i = 0; i < edgesCount; i++ )
            {
                remainingEdgesIndices.Add(i);
            }

            // try to apply the scaffold to a series of connected edges

            tryCounter = 0;

            for(;;)
            {
                Scaffolds.Graph.GraphScaffoldInstanceData instanceData;

                scaffoldEdgesIndices = new List<int>();
                tryToApplyScaffoldToRemainingEdges(graphTreeNode, remainingEdgesIndices, out scaffoldAppliedSuccessfull, scaffoldEdgesIndices, out instanceData);

                if( scaffoldAppliedSuccessfull )
                {
                    tryCounter = 0;

                    // remove all remainingEdgesIndices which are mentioned in scaffoldEdgesIndices

                    foreach( int iterationEdgeIndex in scaffoldEdgesIndices )
                    {
                        remainingEdgesIndices.Remove(iterationEdgeIndex);
                    }

                    // store scaffold edge list away

                    // TODO

                    /*
                    // for now we just rpint it

                    string debugString = "";

                    foreach( int iterationScaffoldEdgeIndex in scaffoldEdgesIndices )
                    {
                        debugString += iterationScaffoldEdgeIndex.ToString() + " ";
                    }

                    Console.WriteLine(debugString);
                     */

                    // BREAK this breaks genericity
                    Line newLine;
                    newLine = new Line();
                    newLine.a = getVertexPositionByIndexDelegate(graphTreeNode,((Scaffolds.Graph.ExtractLineScaffoldInstanceData)instanceData).lastVertexIndex).valueVector2Float;
                    newLine.b = ((Scaffolds.Graph.ExtractLineScaffoldInstanceData)instanceData).getFirstPosition();

                    lines.Add(newLine);
                }

                // we give up if the maximal try count is reached
                if( tryCounter > MAXTRIES )
                {
                    break;
                }

                tryCounter++;
            }
            
            resultCode = EnumResult.OK;
        }

        public override void addOperatorAsParameter(GeneticProgramming.TypeRestrictedOperator parameter)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override GeneticProgramming.TypeRestrictedOperator clone()
        {
            // TODO
            throw new NotImplementedException();
        }

        public override string getTypeAsString()
        {
            return "SearchScaffoldInGraph";
        }

        /**
         * 
         * \param remainingEdgesIndices list with indices which aren't touched by any scaffold, is not changed
         * \param scaffoldAppliedSuccessfull is true if an scaffold was applied successfull
         * \param scaffoldEdges contains all edges of the scaffold, valid only if the application was successful
         */
        private void tryToApplyScaffoldToRemainingEdges(Datastructures.TreeNode graph, List<int> remainingEdgesIndices, out bool scaffoldAppliedSuccessfull, List<int> scaffoldEdges, out Scaffolds.Graph.GraphScaffoldInstanceData instanceData)
        {
            int startEdgeIndex;
            Scaffolds.Graph.GraphScaffold.EnumScaffoldIterationResult scaffoldIterationResult;
            int subsequentIterationsCount;

            // array which tells about remainingEdgesIndices which edges were either consumed or tried
            bool[] consumedEdgeIndex;

            consumedEdgeIndex = new bool[remainingEdgesIndices.Count];

            scaffoldAppliedSuccessfull = false;

            scaffoldEdges.Clear();

            // choose random edge and try to start scaffold from there
            startEdgeIndex = random.Next(remainingEdgesIndices.Count);

            instanceData = searchingScaffoldRoot.newInstanceData();

            scaffoldIterationResult = searchingScaffoldRoot.firstIteration(instanceData, graph, startEdgeIndex, getVertexPositionByIndexDelegate);

            if( scaffoldIterationResult == Scaffolds.Graph.GraphScaffold.EnumScaffoldIterationResult.DOESNTMATCH )
            {
                return;
            }

            consumedEdgeIndex[startEdgeIndex] = true;

            subsequentIterationsCount = 0;

            // is the index of the vertex which was on the last edge consumed
            int lastVertexIndex;

            // chose a random vertex of the first edge as our vantage point
            if( random.Next(2) == 0 )
            {
                Datastructures.TreeNode edgeTreeNode;

                edgeTreeNode = graph.childNodes[GRAPHINDEXEDGES].childNodes[startEdgeIndex];

                lastVertexIndex = edgeTreeNode.childNodes[0].value.valueInt;
            }
            else
            {
                Datastructures.TreeNode edgeTreeNode;

                edgeTreeNode = graph.childNodes[GRAPHINDEXEDGES].childNodes[startEdgeIndex];

                lastVertexIndex = edgeTreeNode.childNodes[1].value.valueInt;
            }

            for(;;)
            {
                int edgeI;
                bool consumedEdge; // indicates if we have consumed an edge in this try

                consumedEdge = false;
                
                // search for a edge which wasn't consumed and contains the lastVertexIndex

                for( edgeI = 0; edgeI < consumedEdgeIndex.Length; edgeI++ )
                {
                    if( consumedEdgeIndex[edgeI] )
                    {
                        continue;
                    }

                    // we are here if the edge was not jet consumed

                    int edgeIndex;
                    Datastructures.TreeNode edgeTreeNode;

                    edgeIndex = remainingEdgesIndices[edgeI];
                    edgeTreeNode = graph.childNodes[GRAPHINDEXEDGES].childNodes[edgeIndex];

                    if( edgeTreeNode.childNodes[0].value.valueInt != lastVertexIndex && edgeTreeNode.childNodes[1].value.valueInt != lastVertexIndex )
                    {
                        continue;
                    }
                    
                    // we are here if this edge is connected with the last vertex

                    consumedEdgeIndex[edgeI] = true;

                    // we try to consume it with the scaffold

                    scaffoldIterationResult = searchingScaffoldRoot.nextIteration(instanceData, graph, edgeIndex, getVertexPositionByIndexDelegate);

                    if (scaffoldIterationResult == Scaffolds.Graph.GraphScaffold.EnumScaffoldIterationResult.DOESNTMATCH)
                    {
                        continue;
                    }

                    // we are here if we found an edge which the scaffold accepted

                    // now we need to
                    // * add it to the solution
                    // * set the new lastVertexIndex

                    scaffoldEdges.Add(edgeIndex);

                    if (edgeTreeNode.childNodes[0].value.valueInt == lastVertexIndex )
                    {
                        lastVertexIndex = edgeTreeNode.childNodes[1].value.valueInt;
                    }
                    else // edgeTreeNode.childNodes[1].value.valueInt == lastVertexIndex)
                    {
                        lastVertexIndex = edgeTreeNode.childNodes[0].value.valueInt;
                    }

                    consumedEdge = true;
                    break;
                }

                // if we haven't consumed an edge we are done, because we don't have any other possibilities
                if( !consumedEdge )
                {
                    break;
                }

                subsequentIterationsCount++;
            }

            // if there was no subsequent application the scaffold was not really applied
            if( subsequentIterationsCount < 1 )
            {
                return;
            }

            scaffoldAppliedSuccessfull = true;
        }

        static private Datastructures.Variadic getVertexPositionByIndexDelegate(Datastructures.TreeNode graph, int vertexIndex)
        {
            Datastructures.TreeNode specificVertexOfGraph;

            specificVertexOfGraph = graph.childNodes[GRAPHINDEXVERTICES].childNodes[vertexIndex];

            return specificVertexOfGraph.childNodes[2].value;
        }

        public struct Line
        {
            public Misc.Vector2<float> a;
            public Misc.Vector2<float> b;
        }

        public List<Line> lines = new List<Line>();

        public Scaffolds.Graph.GraphScaffold searchingScaffoldRoot; /** \brief the root of the scaffold which should be searched in the graph */

        public Random random; // must be set from outside

        private const int GRAPHINDEXVERTICES = 0;
        private const int GRAPHINDEXEDGES = 1; // the edges of a grap are at index 1 of the graph stored
    }
}
