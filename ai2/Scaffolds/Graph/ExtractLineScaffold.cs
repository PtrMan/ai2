using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaffolds.Graph
{
    /**
     * scaffold tries to extract a line from the graph
     * 
     * 
     */
    class ExtractLineScaffold : GraphScaffold
    {
        public override GraphScaffoldInstanceData newInstanceData()
        {
            ExtractLineScaffoldInstanceData result;

            result = new ExtractLineScaffoldInstanceData();

            return result;
        }

        public override EnumScaffoldIterationResult firstIteration(GraphScaffoldInstanceData scaffoldInstanceData, Datastructures.TreeNode graph, int startEdgeIndex, getVertexPositionByIndexDelegate getVertexPositionByIndex)
        {
            ExtractLineScaffoldInstanceData castedInstanceData;
            Datastructures.Variadic variadicPositionA;
            Datastructures.Variadic variadicPositionB;
            int vertexIndexA;
            int vertexIndexB;

            castedInstanceData = (ExtractLineScaffoldInstanceData)scaffoldInstanceData;

            getVertexIndicesOfEdge(graph, startEdgeIndex, out vertexIndexA, out vertexIndexB);

            variadicPositionA = getVertexPositionByIndex(graph, vertexIndexA);
            variadicPositionB = getVertexPositionByIndex(graph, vertexIndexB);

            System.Diagnostics.Debug.Assert(variadicPositionA.type == Datastructures.Variadic.EnumType.VECTOR2FLOAT);
            System.Diagnostics.Debug.Assert(variadicPositionB.type == Datastructures.Variadic.EnumType.VECTOR2FLOAT);

            castedInstanceData.startVertexIndexA = vertexIndexA;
            castedInstanceData.startVertexIndexB = vertexIndexB;

            castedInstanceData.startVertexPositionA = variadicPositionA.valueVector2Float;
            castedInstanceData.startVertexPositionB = variadicPositionB.valueVector2Float;
            
            return EnumScaffoldIterationResult.FULLMATCH;
        }

        public override EnumScaffoldIterationResult nextIteration(GraphScaffoldInstanceData scaffoldInstanceData, Datastructures.TreeNode graph, int edgeIndex, getVertexPositionByIndexDelegate getVertexPositionByIndex)
        {
            ExtractLineScaffoldInstanceData castedInstanceData;
            int vertexIndexA;
            int vertexIndexB;

            castedInstanceData = (ExtractLineScaffoldInstanceData)scaffoldInstanceData;

            getVertexIndicesOfEdge(graph, edgeIndex, out vertexIndexA, out vertexIndexB);

            if( castedInstanceData.startPositionState == ExtractLineScaffoldInstanceData.EnumStartPositionState.NOTDECIDED )
            {
                int newVertexIndex;
                Misc.Vector2<float> newVertexPosition;

                Misc.Vector2<float> vertexPositionFirstA;
                Misc.Vector2<float> vertexPositionFirstB;

                Misc.Vector2<float> firstNormalizedDirection;
                Misc.Vector2<float> newNormalizedDirection;

                float absoluteCosOfAngle;
                float absoluteCosOfAngleOposite; // in the opposite direction

                // we actually measure the new angle to the new point and decide if the angle is too steep
                // if it is, the point doesn't belong to the line

                // NOTE< in the other case we only have to take the normal to the first point into account >

                if( castedInstanceData.startVertexIndexA == vertexIndexA )
                {
                    // new point is vertexIndexB
                    newVertexIndex = vertexIndexB;
                }
                else
                {
                    // new point is vertexIndexA
                    newVertexIndex = vertexIndexA;
                }

                newVertexPosition = getVertexPositionByIndex(graph, newVertexIndex).valueVector2Float;

                // retrive the vertex positions of the first edge stored in castedInstanceData
                vertexPositionFirstA = getVertexPositionByIndex(graph, castedInstanceData.startVertexIndexA).valueVector2Float;
                vertexPositionFirstB = getVertexPositionByIndex(graph, castedInstanceData.startVertexIndexB).valueVector2Float;

                // TODO LATER< try to find cause of groups which points lie on the same position >
                if( (vertexPositionFirstA - vertexPositionFirstB).magnitude() < 0.0001f )
                {
                    return EnumScaffoldIterationResult.DOESNTMATCH;
                }

                float mag = (vertexPositionFirstA - vertexPositionFirstB).magnitude();
                
                firstNormalizedDirection = (vertexPositionFirstA - vertexPositionFirstB).normalized();

                if( (newVertexPosition - vertexPositionFirstA).magnitude() < (newVertexPosition - vertexPositionFirstB).magnitude() )
                {
                    // newVertexPosition is near firstA

                    newNormalizedDirection = (newVertexPosition - vertexPositionFirstA).normalized();

                    castedInstanceData.startPositionState = ExtractLineScaffoldInstanceData.EnumStartPositionState.B;
                }
                else
                {
                    // newVertexPosition is near firstB

                    newNormalizedDirection = (newVertexPosition - vertexPositionFirstB).normalized();

                    castedInstanceData.startPositionState = ExtractLineScaffoldInstanceData.EnumStartPositionState.A;
                }

                absoluteCosOfAngle = System.Math.Abs(newNormalizedDirection.dot(firstNormalizedDirection));

                // rotate it 180 degrees
                firstNormalizedDirection = new Misc.Vector2<float>() - firstNormalizedDirection;

                absoluteCosOfAngleOposite = System.Math.Abs(newNormalizedDirection.dot(firstNormalizedDirection));

                if (absoluteCosOfAngle < 0.3 && absoluteCosOfAngleOposite < 0.3)
                {
                    // new point has a too steep angle
                    return EnumScaffoldIterationResult.DOESNTMATCH;
                }
                else
                {
                    // set all variables of the castedInstanceData
                    castedInstanceData.lastVertexIndex = newVertexIndex;

                    return EnumScaffoldIterationResult.FULLMATCH;
                }
            }
            else
            {
                int newLastVertexIndex;
                Misc.Vector2<float> newLastVertexPosition;
                Misc.Vector2<float> oldLastVertexPosition;
                
                Misc.Vector2<float> newNormalizedDirection;
                Misc.Vector2<float> lineDirection;

                float absoluteCosOfAngle;

                if( castedInstanceData.lastVertexIndex == vertexIndexA )
                {
                    newLastVertexIndex = vertexIndexB;
                }
                else
                {
                    newLastVertexIndex = vertexIndexA;
                }


                newLastVertexPosition = getVertexPositionByIndex(graph, newLastVertexIndex).valueVector2Float;
                oldLastVertexPosition = getVertexPositionByIndex(graph, castedInstanceData.lastVertexIndex).valueVector2Float;

                newNormalizedDirection = (newLastVertexPosition - oldLastVertexPosition).normalized();
                lineDirection = (castedInstanceData.getFirstPosition() - oldLastVertexPosition).normalized();

                absoluteCosOfAngle = System.Math.Abs(lineDirection.dot(newNormalizedDirection));
                
                if (absoluteCosOfAngle < 0.3)
                {
                    // new point has a too steep angle
                    return EnumScaffoldIterationResult.DOESNTMATCH;
                }
                // we are here if it does match

                castedInstanceData.lastVertexIndex = newLastVertexIndex;

                return EnumScaffoldIterationResult.FULLMATCH;
            }
        }


        private static void getVertexIndicesOfEdge(Datastructures.TreeNode graph, int edgeIndex, out int vertexIndexA, out int vertexIndexB)
        {
            Datastructures.TreeNode edgeTreeNode;

            edgeTreeNode = graph.childNodes[GRAPHINDEXEDGES].childNodes[edgeIndex];

            vertexIndexA = edgeTreeNode.childNodes[0].value.valueInt;
            vertexIndexB = edgeTreeNode.childNodes[1].value.valueInt;
        }

        private const int GRAPHINDEXVERTICES = 0;
        private const int GRAPHINDEXEDGES = 1; // the edges of a grap are at index 1 of the graph stored
    }
}
