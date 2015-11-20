using System;
using System.Collections.Generic;

using Misc;

namespace Operators.Visual
{
    /**
     * groups points into grids
     * 
     * the grids do have only a size and a offset, no transformations are applied (jet)
     * 
     * it is assumed that the x coordinates are in the range of [0-1]
     * 
     * the output is a list with elements which do have
     * * [0] a list with all points
     * * [1] a simple list with
     *  * [0] (int) x index
     *  * [1] (int) y index
     * * [2] (vector2<float>) the middle/average position
     */
    class GroupPointsGrid
    {
        public void call(List<Datastructures.Variadic> parameters, out Datastructures.Variadic result, out GeneticProgramming.TypeRestrictedOperator.EnumResult resultCode)
        {
            int i;
            List<Vector2<float>>[] groups;
            int groupI;
            Datastructures.TreeNode groupsTreeNode;

            resultCode = GeneticProgramming.TypeRestrictedOperator.EnumResult.FAILED;

            groupsTreeNode = new Datastructures.TreeNode();

            result = new Datastructures.Variadic(Datastructures.Variadic.EnumType.STORAGEALGORITHMICCONCEPTTREE);
            result.valueTree = groupsTreeNode;

            // initialize groups
            groups = new List<Vector2<float>>[cachedGroupArraySize.x * cachedGroupArraySize.y];
            for( i = 0; i < cachedGroupArraySize.x * cachedGroupArraySize.y; i++ )
            {
                groups[i] = new List<Vector2<float>>();
            }

            if( parameters.Count < 1 || parameters[0].type != Datastructures.Variadic.EnumType.STORAGEALGORITHMICCONCEPTTREE )
            {
                // TODO< set error >

                return;
            }

            // go through each point
            foreach( Datastructures.TreeNode iterationPointNode in parameters[0].valueTree.childNodes )
            {
                Vector2<float> iterationPoint;
                int groupIndex;
                
                if( iterationPointNode.value.type != Datastructures.Variadic.EnumType.VECTOR2FLOAT )
                {
                    // TODO< set error >

                    return;
                }

                iterationPoint = iterationPointNode.value.valueVector2Float;

                groupIndex = getGroupIndexOfCoordinate(iterationPoint);

                groups[groupIndex].Add(iterationPoint);
            }

            // build the result
            for( groupI = 0; groupI < groups.Length; groupI++ )
            {
                Datastructures.TreeNode pointsTreeNode;
                Datastructures.TreeNode outputElementTreeNode;

                Vector2<float> center; // middle/average of all points

                center = new Vector2<float>();

                if( groups[groupI].Count == 0 )
                {
                    continue;
                }

                // we are here if a element is in the group

                pointsTreeNode = new Datastructures.TreeNode();

                foreach( Vector2<float> iterationPoint in groups[groupI] )
                {
                    Datastructures.TreeNode treeNodeForPoint;

                    treeNodeForPoint = new Datastructures.TreeNode();
                    treeNodeForPoint.value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.VECTOR2FLOAT);
                    treeNodeForPoint.value.valueVector2Float = iterationPoint;

                    pointsTreeNode.childNodes.Add(treeNodeForPoint);


                    center += iterationPoint;
                }

                center.scale(1.0f / (float)groups[groupI].Count);

                // compose tree for the group
                {
                    Datastructures.TreeNode coordinatesTreeNode;
                    Datastructures.TreeNode centerTreeNode;

                    centerTreeNode = new Datastructures.TreeNode();
                    centerTreeNode.value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.VECTOR2FLOAT);
                    centerTreeNode.value.valueVector2Float = center;

                    coordinatesTreeNode = new Datastructures.TreeNode();
                    coordinatesTreeNode.childNodes.Add(new Datastructures.TreeNode());
                    coordinatesTreeNode.childNodes.Add(new Datastructures.TreeNode());

                    coordinatesTreeNode.childNodes[0].value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
                    coordinatesTreeNode.childNodes[1].value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);

                    coordinatesTreeNode.childNodes[0].value.valueInt = groupI % cachedGroupArraySize.x;
                    coordinatesTreeNode.childNodes[1].value.valueInt = groupI / cachedGroupArraySize.x;

                    outputElementTreeNode = new Datastructures.TreeNode();
                    outputElementTreeNode.childNodes.Add(pointsTreeNode);
                    outputElementTreeNode.childNodes.Add(coordinatesTreeNode);
                    outputElementTreeNode.childNodes.Add(centerTreeNode);
                }

                groupsTreeNode.childNodes.Add(outputElementTreeNode);
            }

            resultCode = GeneticProgramming.TypeRestrictedOperator.EnumResult.OK;
        }

        public void initialize()
        {
            cachedGroupArraySize = new Vector2<int>();
            cachedGroupArraySize.x = (int)(1.0 / cellSize.x) + 1;
            cachedGroupArraySize.y = (int)(1.0 / cellSize.y) + 1;
        }

        private int getGroupIndexOfCoordinate(Vector2<float> coordinate)
        {
            int indexX, indexY;
            
            coordinate = coordinate - cellOffset;

            indexX = (int)(coordinate.x / cellSize.x);
            indexY = (int)(coordinate.y / cellSize.y);

            System.Diagnostics.Debug.Assert(indexX >= 0 && indexX < cachedGroupArraySize.x);
            System.Diagnostics.Debug.Assert(indexY >= 0 && indexY < cachedGroupArraySize.y);

            return indexX + cachedGroupArraySize.x * indexY;
        }

        public float sizeY = 0.0f;

        public Vector2<float> cellOffset;
        public Vector2<float> cellSize;

        private Vector2<int> cachedGroupArraySize;
    }
}
