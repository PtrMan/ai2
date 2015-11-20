using System;
using System.Collections.Generic;

namespace Scaffolds.Graph
{
    /**
     * the scaffolds are modelled after the paper in
     * "Model Construction in General Intelligence"
     * 
     * This class is the baseclass for all Graph based Scaffolds
     */ 
    abstract class GraphScaffold
    {
        public enum EnumScaffoldIterationResult
        {
            FULLMATCH = 0,
	        VARIATION = 2,   // the scaffold did chose another variation of itself
	                         // it means that it matches but only with a variation
            	             // the system above the raw scaffold evaluation can notice that and try to decide another variation or another scaffold of the same family
	        DOESNTMATCH = 1 // the scaffold doesn't match
        }

        public delegate Datastructures.Variadic getVertexPositionByIndexDelegate(Datastructures.TreeNode graph, int vertexIndex);

        public abstract GraphScaffoldInstanceData newInstanceData();

        /**
         * 
         * tries to initialize the scaffold to fit to the first datapoint
         *
         * \param scaffoldInstanceData is the data for the instance of this scaffold
         * \param graph is the graph the scaffold operates at
         * \param startEdgeIndex is the edge index in the graph of the start
         * \param getVertexPositionByIndex gets as the parameter the vertex index and must return the position (as a variadic for flexibility)
         */
	    public abstract EnumScaffoldIterationResult firstIteration(GraphScaffoldInstanceData scaffoldInstanceData, Datastructures.TreeNode graph, int startEdgeIndex, getVertexPositionByIndexDelegate getVertexPositionByIndex);
		
        /**
         * 
         * checks the scaffold against the next element
         * 
	     * \param scaffoldInstanceData is the data for the instance of this scaffold
         * \param graph is the graph the scaffold operates at
         * \param edgeIndex is the edge index in the graph (is connected to the previous edge)
         * \param getVertexPositionByIndex gets as the parameter the vertex index and must return the position (as a variadic for flexibility)
         */
	    public abstract EnumScaffoldIterationResult nextIteration(GraphScaffoldInstanceData scaffoldInstanceData, Datastructures.TreeNode graph, int edgeIndex, getVertexPositionByIndexDelegate getVertexPositionByIndex);
    }
}
