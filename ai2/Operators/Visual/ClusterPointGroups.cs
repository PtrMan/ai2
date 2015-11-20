using System;
using System.Collections.Generic;

namespace Operators.Visual
{
    class ClusterPointGroups
    {
        /**
         * this operator clusters groups to clusters
         * (really simple and stupid)
         * 
         * parameters
         *  * list with groups, which can contain points
         *  
         *  the points must be float
         * 
         * output
         *  * list with clustered groups
         *  
         *  each group is is of the form
         *  | \
         *  |    \ 
         *  |       \
         *  |          \
         *  |             \
         *  |                \
         * points           center position
         */
        // uncommented because its not tested, and used
        // TODOD REMOVE
        /*
        public void call(List<Datastructures.Variadic> parameters, out Datastructures.Variadic result, out GeneticProgramming.TypeRestrictedOperator.EnumResult resultCode)
        {
            Datastructures.TreeNode groupTree;

            groupTree = new Datastructures.TreeNode();

            result = new Datastructures.Variadic(Datastructures.Variadic.EnumType.STORAGEALGORITHMICCONCEPTTREE);
            result.valueTree = groupTree;

            if( parameters.Count < 1 )
            {
                // TODO< return error >

                return;
            }

            if( parameters[0].type != Datastructures.Variadic.EnumType.STORAGEALGORITHMICCONCEPTTREE )
            {
                // TODO< return error >

                return;
            }

            // TODO< set result code >

            // go through each group
            
            foreach( Datastructures.TreeNode groupTreeNode in parameters[0].valueTree.childNodes )
            {
                Datastructures.TreeNode groupNode;
                Datastructures.TreeNode pointsNode;
                Datastructures.TreeNode centerPositionNode;

                centerPositionNode = new Datastructures.TreeNode();
                centerPositionNode.value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.VECTOR2FLOAT);

                groupNode = new Datastructures.TreeNode();

                pointsNode = new Datastructures.TreeNode();

                groupNode.childNodes.Add(pointsNode);
                groupNode.childNodes.Add(centerPositionNode);

                // go through each point and
                // - add it to the group (points node)
                // - add the position to center position for averaging

                foreach( Datastructures.TreeNode iterationPointTreeNode in groupTreeNode.childNodes )
                {
                    if( iterationPointTreeNode.value.type != Datastructures.Variadic.EnumType.VECTOR2FLOAT )
                    {
                        // TODO< set error >
                        return;
                    }

                    pointsNode.childNodes.Add(new Datastructures.TreeNode());
                    pointsNode.childNodes[0].value = iterationPointTreeNode.value;

                    centerPositionNode.value.valueVector2Float.x += iterationPointTreeNode.value.valueVector2Float.x;
                    centerPositionNode.value.valueVector2Float.y += iterationPointTreeNode.value.valueVector2Float.y;
                }

                // average the positions
                if( groupTreeNode.childNodes.Count > 0 )
                {
                    centerPositionNode.value.valueVector2Float.x /= (float)groupTreeNode.childNodes.Count;
                    centerPositionNode.value.valueVector2Float.y /= (float)groupTreeNode.childNodes.Count;
                }

                groupTree.childNodes.Add(groupNode);
            }
        }
         */
    }
}
