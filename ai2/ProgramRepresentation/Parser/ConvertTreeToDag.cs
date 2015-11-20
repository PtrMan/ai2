using System;
using System.Collections.Generic;

namespace ProgramRepresentation.Parser
{
    /**
     * 
     * Converts the parse tree of a functional program/expression to a equal dag representation
     * this dag representation can be either interpreted or converted to a imperative dag representation, which can be converted to C#, C++, openCL, cuda, etc.
     * 
     */
    class ConvertTreeToDag
    {
        static public void convertRecursive(Datastructures.Dag<DagElementData> dag, Functional.ParseTreeElement sourceTree, out int dagRootElementIndex)
        {
            DagElementData createdDagElementData;
            Datastructures.Dag<DagElementData>.Element createdDagElement;

            if( sourceTree.type == Functional.ParseTreeElement.EnumType.SCOPE )
            {
                createdDagElementData = new DagElementData();
                createdDagElementData.type = DagElementData.EnumType.FSCOPE;
                createdDagElementData.valueInt = (int)((Functional.ScopeParseTreeElement)sourceTree).scopeType;

                createdDagElement = new Datastructures.Dag<DagElementData>.Element(createdDagElementData);

                dag.addElement(createdDagElement);

                dagRootElementIndex = dag.elements.Count-1;

                // add all childrens to the dag
                createdDagElement.childIndices = convertListOfTreeElementsToDagElements(dag, sourceTree.childrens);

                return;
            }
            else if( sourceTree.type == Functional.ParseTreeElement.EnumType.NUMBER )
            {
                Functional.NumberParseTreeElement numberParseTreeElement;

                numberParseTreeElement = (Functional.NumberParseTreeElement)sourceTree;

                if( numberParseTreeElement.numberType == Functional.NumberParseTreeElement.EnumNumberType.INTEGER )
                {
                    createdDagElementData = new DagElementData();
                    createdDagElementData.type = DagElementData.EnumType.CONSTINT;
                    createdDagElementData.valueInt = numberParseTreeElement.valueInt;
                    
                    createdDagElement = new Datastructures.Dag<DagElementData>.Element(createdDagElementData);

                    dag.addElement(createdDagElement);

                    dagRootElementIndex = dag.elements.Count-1;

                    return;
                }
                else if (numberParseTreeElement.numberType == Functional.NumberParseTreeElement.EnumNumberType.FLOAT)
                {
                    createdDagElementData = new DagElementData();
                    createdDagElementData.type = DagElementData.EnumType.CONSTFLOAT;
                    createdDagElementData.valueFloat = numberParseTreeElement.valueFloat;

                    createdDagElement = new Datastructures.Dag<DagElementData>.Element(createdDagElementData);

                    dag.addElement(createdDagElement);

                    dagRootElementIndex = dag.elements.Count - 1;

                    return;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false, "Unhandled type!");
                    throw new Exception("Internal Error!");
                }
            }
            else if( sourceTree.type == Functional.ParseTreeElement.EnumType.IDENTIFIER )
            {
                Functional.IdentifierParseTreeElement identifierParseTreeElement;

                identifierParseTreeElement = (Functional.IdentifierParseTreeElement)sourceTree;

                createdDagElementData = new DagElementData();
                createdDagElementData.type = DagElementData.EnumType.IDENTIFIERNAME;
                createdDagElementData.identifier = identifierParseTreeElement.identifier;
                    
                createdDagElement = new Datastructures.Dag<DagElementData>.Element(createdDagElementData);

                dag.addElement(createdDagElement);

                dagRootElementIndex = dag.elements.Count-1;

                return;
            }
            else if( sourceTree.type == Functional.ParseTreeElement.EnumType.ARRAY )
            {
                createdDagElementData = new DagElementData();
                createdDagElementData.type = DagElementData.EnumType.FARRAY;

                createdDagElement = new Datastructures.Dag<DagElementData>.Element(createdDagElementData);

                dag.addElement(createdDagElement);

                dagRootElementIndex = dag.elements.Count - 1;

                // add all childrens to the dag
                createdDagElement.childIndices = convertListOfTreeElementsToDagElements(dag, sourceTree.childrens);

                return;
            }
            else if( sourceTree.type == Functional.ParseTreeElement.EnumType.STRING )
            {
                Functional.StringParseTreeElement stringParseTreeElement;

                stringParseTreeElement = (Functional.StringParseTreeElement)sourceTree;

                createdDagElementData = new DagElementData();
                createdDagElementData.type = DagElementData.EnumType.CONSTSTRING;

                createdDagElementData.valueString = stringParseTreeElement.content;

                createdDagElement = new Datastructures.Dag<DagElementData>.Element(createdDagElementData);

                dag.addElement(createdDagElement);

                dagRootElementIndex = dag.elements.Count - 1;

                return;
            }
            else if( sourceTree.type == Functional.ParseTreeElement.EnumType.BOOLEAN )
            {
                Functional.BooleanParseTreeElement booleanParseTreeElement;

                booleanParseTreeElement = (Functional.BooleanParseTreeElement)sourceTree;

                createdDagElementData = new DagElementData();
                createdDagElementData.type = DagElementData.EnumType.CONSTBOOL;

                createdDagElementData.valueBool = booleanParseTreeElement.booleanType == Functional.BooleanParseTreeElement.EnumBooleanType.TRUE;

                createdDagElement = new Datastructures.Dag<DagElementData>.Element(createdDagElementData);

                dag.addElement(createdDagElement);

                dagRootElementIndex = dag.elements.Count - 1;

                return;
            }


            throw new Exception("Unreachable Code");
        }

        /**
         * 
         * \result are the childIndices
         */
        static private List<int> convertListOfTreeElementsToDagElements(Datastructures.Dag<DagElementData> dag, List<Functional.ParseTreeElement> list)
        {
            List<int> childIndices;

            childIndices = new List<int>();

            foreach (Functional.ParseTreeElement iterationParseTreeElement in list)
            {
                int childrenDagIndex;

                convertRecursive(dag, iterationParseTreeElement, out childrenDagIndex);

                childIndices.Add(childrenDagIndex);
            }

            return childIndices;
        }
    }
}
