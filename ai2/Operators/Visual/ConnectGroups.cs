using System;
using System.Collections.Generic;

using Misc;

namespace Operators.Visual
{
    /**
     * This algorithm tries to connect the groups into group relating graphs
     * 
     * it assumes that both grids do have the same size
     * 
     * the parameters are the lists which contain the points
     * 
     * the result is a graph-structure where the elements get copied from the input, so the sematic informations are transparently transfered
     * 
     */
    class ConnectGroups : GeneticProgramming.TypeRestrictedOperator
    {

        public override void call(List<Datastructures.Variadic> parameters, List<Datastructures.Variadic> globals, out Datastructures.Variadic result, out GeneticProgramming.TypeRestrictedOperator.EnumResult resultCode)
        {
            Datastructures.TreeNode resultGraph;
            Datastructures.TreeNode grid0;
            Datastructures.TreeNode grid1;

            // this are arrays wih the indices of the elements in the graph from the elements in the Grid
            // it has the size of gridSizeX * gridsizeY
            // is -1 if the grid element wasn't saved allready inside the graph
            int[] groupsGraphElementIndicesForGrid0;
            int[] groupsGraphElementIndicesForGrid1;

            groupsGraphElementIndicesForGrid0 = null;
            groupsGraphElementIndicesForGrid1 = null;

            resultCode = GeneticProgramming.TypeRestrictedOperator.EnumResult.FAILED;
            result = null;

            // we accept at the current time only two inputs
            if(
                parameters.Count != 2 ||
                parameters[0].type != Datastructures.Variadic.EnumType.STORAGEALGORITHMICCONCEPTTREE ||
                parameters[1].type != Datastructures.Variadic.EnumType.STORAGEALGORITHMICCONCEPTTREE
            )
            {
                return;
            }

            grid0 = parameters[0].valueTree;
            grid1 = parameters[1].valueTree;

            // initialize result graph
            resultGraph = new Datastructures.TreeNode();
            resultGraph.childNodes.Add(new Datastructures.TreeNode());
            resultGraph.childNodes.Add(new Datastructures.TreeNode());
            resultGraph.childNodes.Add(new Datastructures.TreeNode());

            result = new Datastructures.Variadic(Datastructures.Variadic.EnumType.STORAGEALGORITHMICCONCEPTTREE);
            result.valueTree = resultGraph;

            initializeIndicesOfGrid(ref groupsGraphElementIndicesForGrid0);
            initializeIndicesOfGrid(ref groupsGraphElementIndicesForGrid1);

            storeGridElementsIntoGraph(ref groupsGraphElementIndicesForGrid0, grid0, resultGraph.childNodes[GRAPHINDEXVERTICES]);
            storeGridElementsIntoGraph(ref groupsGraphElementIndicesForGrid1, grid1, resultGraph.childNodes[GRAPHINDEXVERTICES]);

            // go through each element in grid0 and search for new possible connections to grid1
            searchForNewPossibleConnectionsBetweenGrids(ref groupsGraphElementIndicesForGrid0, ref groupsGraphElementIndicesForGrid1, grid0, grid1, resultGraph.childNodes[GRAPHINDEXEDGES], resultGraph.childNodes[GRAPHINDEXVERTICES]);

            // go through each element in grid1 and search for new possible connections to grid0
            searchForNewPossibleConnectionsBetweenGrids(ref groupsGraphElementIndicesForGrid1, ref groupsGraphElementIndicesForGrid0, grid1, grid0, resultGraph.childNodes[GRAPHINDEXEDGES], resultGraph.childNodes[GRAPHINDEXVERTICES]);

            resultCode = GeneticProgramming.TypeRestrictedOperator.EnumResult.OK;
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
            return "ConnectedGroups";
        }

        private void initializeIndicesOfGrid(ref int[] indices)
        {
            int i;

            indices = new int[gridsize.x*gridsize.y];
            for( i = 0; i < gridsize.x*gridsize.y; i++ )
            {
                indices[i] = -1;
            }
        }

        private static bool isConnectionAllreadyInGraph(Datastructures.TreeNode graphEdges, int elementIndexA, int elementIndexB)
        {
            foreach( Datastructures.TreeNode iterationEdge in graphEdges.childNodes )
            {
                if(
                    (iterationEdge.childNodes[0].value.valueInt == elementIndexA && iterationEdge.childNodes[1].value.valueInt == elementIndexB) ||
                    (iterationEdge.childNodes[0].value.valueInt == elementIndexB && iterationEdge.childNodes[1].value.valueInt == elementIndexA)
                )
                {
                    return true;
                }
            }

            return false;
        }

        /**
         * 
         * stores all grid elements into the graph, even the ones which aren't linked
         * 
         */
        private void storeGridElementsIntoGraph(ref int[] gridIndicesTable, Datastructures.TreeNode grid, Datastructures.TreeNode graphVertices)
        {
            foreach( Datastructures.TreeNode gridElement in grid.childNodes )
            {
                int gridX, gridY;
                int indexInArray;

                gridX = gridElement.childNodes[1].childNodes[0].value.valueInt;
                gridY = gridElement.childNodes[1].childNodes[1].value.valueInt;

                indexInArray = translateGridCoordinatesToIndex(gridX, gridY);

                System.Diagnostics.Debug.Assert(gridIndicesTable[indexInArray]==-1, "grid table was allready initialized");
                
                graphVertices.childNodes.Add(gridElement);

                gridIndicesTable[indexInArray] = graphVertices.childNodes.Count-1;
            }
        }

        private void searchForNewPossibleConnectionsBetweenGrids(ref int[] indicesGridA, ref int[] indicesGridB, Datastructures.TreeNode gridA, Datastructures.TreeNode gridB, Datastructures.TreeNode graphEdges, Datastructures.TreeNode graphVertices)
        {
            int gridX;
            int gridY;

            for( gridY = 1; gridY < gridsize.y-1; gridY++ )
            {
                for( gridX = 1; gridX < gridsize.x-1; gridX++ )
                {
                    Vector2<float> centerA, centerB;
                    
                    if (indicesGridA[translateGridCoordinatesToIndex(gridX,gridY)] == -1)
                    {
                        continue;
                    }

                    centerA = graphVertices.childNodes[ indicesGridA[translateGridCoordinatesToIndex(gridX,gridY)] ].childNodes[2].value.valueVector2Float;

                    // search connections in all directions
                    if (indicesGridB[translateGridCoordinatesToIndex(gridX-1, gridY-1)] != -1)
                    {
                        centerB = graphVertices.childNodes[indicesGridB[translateGridCoordinatesToIndex(gridX - 1, gridY - 1)]].childNodes[2].value.valueVector2Float;

                        if ((centerA - centerB).magnitude() < maxConnectionDistance && (centerA - centerB).magnitude() > 0.0001f )
                        {
                            addNewEdge(indicesGridA[gridX + gridY * gridsize.x], indicesGridB[gridX - 1 + (gridY - 1) * gridsize.x], graphEdges);
                        }

                    }


                    if (indicesGridB[translateGridCoordinatesToIndex(gridX, gridY-1)] != -1)
                    {
                        centerB = graphVertices.childNodes[indicesGridB[translateGridCoordinatesToIndex(gridX, gridY - 1)]].childNodes[2].value.valueVector2Float;

                        if ((centerA - centerB).magnitude() < maxConnectionDistance && (centerA - centerB).magnitude() > 0.0001f)
                        {
                            addNewEdge(indicesGridA[gridX + gridY * gridsize.x], indicesGridB[gridX + (gridY - 1) * gridsize.x], graphEdges);
                        }
                    }


                    if (indicesGridB[translateGridCoordinatesToIndex(gridX-1, gridY)] != -1)
                    {
                        centerB = graphVertices.childNodes[indicesGridB[translateGridCoordinatesToIndex(gridX - 1, gridY)]].childNodes[2].value.valueVector2Float;

                        if ((centerA - centerB).magnitude() < maxConnectionDistance && (centerA - centerB).magnitude() > 0.0001f)
                        {
                            addNewEdge(indicesGridA[gridX + gridY * gridsize.x], indicesGridB[gridX - 1 + (gridY) * gridsize.x], graphEdges);
                        }
                    }
                }
            }

        }

        // adds a new edge between the indices to the graph-edges
        private static void addNewEdge(int indexA, int indexB, Datastructures.TreeNode graphEdges)
        {
            Datastructures.TreeNode newEdge;

            newEdge = new Datastructures.TreeNode();
            newEdge.childNodes.Add(new Datastructures.TreeNode());
            newEdge.childNodes.Add(new Datastructures.TreeNode());

            newEdge.childNodes[0].value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
            newEdge.childNodes[1].value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);

            newEdge.childNodes[0].value.valueInt = indexA;
            newEdge.childNodes[1].value.valueInt = indexB;

            graphEdges.childNodes.Add(newEdge);
        }

        private int translateGridCoordinatesToIndex(int x, int y)
        {
            System.Diagnostics.Debug.Assert(x >= 0 && x < gridsize.x);
            System.Diagnostics.Debug.Assert(y >= 0 && y < gridsize.y);

            return x + gridsize.x*y;
        }

        public Vector2<int> gridsize;

        public float maxConnectionDistance = float.MaxValue;

        private const int GRAPHINDEXVERTICES = 0;
        private const int GRAPHINDEXEDGES = 1; // the edges of a grap are at index 1 of the graph stored
        
    }
}
