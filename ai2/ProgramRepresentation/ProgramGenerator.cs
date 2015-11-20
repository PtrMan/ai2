using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProgramRepresentation
{
    class ProgramGenerator
    {
        [Serializable]
        public class TranslationException : Exception
        {
            // Constructors
            public TranslationException()
                : base("TranslationException")
            { }

            // Ensure Exception is Serializable
            protected TranslationException(SerializationInfo info, StreamingContext ctxt)
                : base(info, ctxt)
            { }
        }

        // in this class all context relevant informations are stored, which are needed for the creation of the text of the kernel
        private class ProgramGenerationContext
        {
            // size of the input map
            // only valid for image/map based algorithms
            public int inputMapWidth;

            public Dictionary<string, int> widthOfArrays = new Dictionary<string, int>();
        }

        public interface IProgramGenerationCallbacks
        {
            string generateStringForConstantPi();
        }

        public ProgramGenerator(IProgramGenerationCallbacks callbacks)
        {
            this.callbacks = callbacks;
        }

        private IProgramGenerationCallbacks callbacks;

        public string generateSource(Program program, int inputMapSize, Dictionary<string, int> widthOfArrays)
        {
            int entryIndex;
            ProgramGenerationContext programGenerationContext;

            programGenerationContext = new ProgramGenerationContext();
            programGenerationContext.inputMapWidth = inputMapSize;
            programGenerationContext.widthOfArrays = widthOfArrays;

            entryIndex = 0;

            return recursivlyGenerateStringFor(program.dag, entryIndex, programGenerationContext);
        }

        // throws TranslationException
        private string recursivlyGenerateStringFor(Datastructures.Dag<DagElementData> dag, int elementIndex, ProgramGenerationContext programGenerationContext)
        {
            string resultString;

            resultString = "";

            if (dag.elements[elementIndex].content.type == DagElementData.EnumType.CONSTLOOP)
            {
                string counterName;
                string constantBeginString;
                string constantEndString;

                counterName = getIdentifierOfElement(dag, dag.elements[elementIndex].childIndices[2]);
                constantBeginString = getStringOfConstInt(dag, dag.elements[elementIndex].childIndices[0]);
                constantEndString = getStringOfConstInt(dag, dag.elements[elementIndex].childIndices[1]);

                resultString = string.Format("for({0} {1} = {2}; {1} < {3}; {1}++)\n{{\n", "int", counterName, constantBeginString, constantEndString);
                resultString += recursivlyGenerateStringFor(dag, dag.elements[elementIndex].childIndices[3], programGenerationContext);

                resultString += "}\n";

                return resultString;
            }
            else if (dag.elements[elementIndex].content.type == DagElementData.EnumType.SEQUENCE)
            {
                resultString = "{\n";

                foreach (int childDagElementIndex in dag.elements[elementIndex].childIndices)
                {
                    resultString += recursivlyGenerateStringFor(dag, childDagElementIndex, programGenerationContext) + ";\n";
                }

                resultString += "}\n";

                return resultString;
            }
            else if (dag.elements[elementIndex].content.type == DagElementData.EnumType.ALLOCATESET)
            {
                string typeOfVariable;
                string nameOfVariableToSet;
                string valueOfVariable;

                typeOfVariable = getStringOfTypeOf(dag, dag.elements[elementIndex].childIndices[1]);
                nameOfVariableToSet = getIdentifierOfElement(dag, dag.elements[elementIndex].childIndices[0]);

                if (dag.elements[dag.elements[elementIndex].childIndices[1]].content.type == DagElementData.EnumType.CONSTINT)
                {
                    valueOfVariable = getStringOfConstInt(dag, dag.elements[elementIndex].childIndices[1]);
                }
                else if (dag.elements[dag.elements[elementIndex].childIndices[1]].content.type == DagElementData.EnumType.CONSTFLOAT)
                {
                    valueOfVariable = getStringOfConstFloat(dag, dag.elements[elementIndex].childIndices[1]);
                }
                else
                {
                    throw new TranslationException();
                }

                return string.Format("{0} {1} = {2};\n", typeOfVariable, nameOfVariableToSet, valueOfVariable);
            }
            else if (dag.elements[elementIndex].content.type == DagElementData.EnumType.ADDASSIGN)
            {
                string nameOfVariableToSet;
                string stringOfRightSide;

                nameOfVariableToSet = getIdentifierOfElement(dag, dag.elements[elementIndex].childIndices[0]);
                stringOfRightSide = recursivlyGenerateStringFor(dag, dag.elements[elementIndex].childIndices[1], programGenerationContext);

                return nameOfVariableToSet + " += (" + stringOfRightSide + ");\n";
            }
            else if (dag.elements[elementIndex].content.type == DagElementData.EnumType.READINPUTAT2D)
            {
                string xParameterString;
                string yParameterString;

                xParameterString = recursivlyGenerateStringFor(dag, dag.elements[elementIndex].childIndices[0], programGenerationContext);
                yParameterString = recursivlyGenerateStringFor(dag, dag.elements[elementIndex].childIndices[1], programGenerationContext);

                //return "readInputAt(" + xParameterString + "," + yParameterString + ")";
                return string.Format("inputMap[{0} + {1}*{2}]", xParameterString, programGenerationContext.inputMapWidth, yParameterString);
            }
            else if (dag.elements[elementIndex].content.type == DagElementData.EnumType.WRITERESULT)
            {
                string resultName;

                resultName = getIdentifierOfElement(dag, dag.elements[elementIndex].childIndices[0]);

                return string.Format("resultMap[indexX] = {0};\n", resultName);
            }
            else if (dag.elements[elementIndex].content.type == DagElementData.EnumType.IDENTIFIERNAME)
            {
                return dag.elements[elementIndex].content.identifier;
            }
            else if (dag.elements[elementIndex].content.type == DagElementData.EnumType.ARRAYREAD2D)
            {
                string xParameterString;
                string yParameterString;
                string arrayName;

                arrayName = dag.elements[elementIndex].content.identifier;

                xParameterString = recursivlyGenerateStringFor(dag, dag.elements[elementIndex].childIndices[0], programGenerationContext);
                yParameterString = recursivlyGenerateStringFor(dag, dag.elements[elementIndex].childIndices[1], programGenerationContext);

                return string.Format("{3}[{0} + {1} * {2}]", xParameterString, yParameterString, programGenerationContext.widthOfArrays[arrayName], arrayName);
            }
            else if (dag.elements[elementIndex].content.type == DagElementData.EnumType.ARRAYREAD1D)
            {
                string xParameterString;
                string arrayName;

                arrayName = dag.elements[elementIndex].content.identifier;

                xParameterString = recursivlyGenerateStringFor(dag, dag.elements[elementIndex].childIndices[0], programGenerationContext);

                return string.Format("{0}[{1}]", arrayName, xParameterString);
            }
            else if (dag.elements[elementIndex].content.type == DagElementData.EnumType.ASSIGNMENTINT)
            {
                string identifier;
                string valueString;

                identifier = getIdentifierOfElement(dag, dag.elements[elementIndex].childIndices[0]);
                valueString = recursivlyGenerateStringFor(dag, dag.elements[elementIndex].childIndices[1], programGenerationContext);

                return string.Format("int {0} = {1};\n", identifier, valueString);
            }
            else if (dag.elements[elementIndex].content.type == DagElementData.EnumType.CONSTINT)
            {
                return getStringOfConstInt(dag, elementIndex);
            }
            else if (dag.elements[elementIndex].content.type == DagElementData.EnumType.CONSTFLOAT)
            {
                return getStringOfConstFloat(dag, elementIndex);
            }
            else if( dag.elements[elementIndex].content.type == DagElementData.EnumType.NULL )
            {
                return recursivlyGenerateStringFor(dag, dag.elements[elementIndex].childIndices[0], programGenerationContext);
            }
            else if(
                dag.elements[elementIndex].content.type == DagElementData.EnumType.SUB ||
                dag.elements[elementIndex].content.type == DagElementData.EnumType.DIV ||
                dag.elements[elementIndex].content.type == DagElementData.EnumType.ADD ||
                dag.elements[elementIndex].content.type == DagElementData.EnumType.MUL
            )
            {
                string leftSideString;
                string rightSideString;
                string operationAsString;

                switch( dag.elements[elementIndex].content.type )
                {
                    case DagElementData.EnumType.ADD:
                    operationAsString = "+";
                    break;

                    case DagElementData.EnumType.SUB:
                    operationAsString = "-";
                    break;

                    case DagElementData.EnumType.MUL:
                    operationAsString = "*";
                    break;

                    case DagElementData.EnumType.DIV:
                    operationAsString = "/";
                    break;

                    default:
                    operationAsString = "";
                    throw new Exception("Internal Error!");
                }

                leftSideString = recursivlyGenerateStringFor(dag, dag.elements[elementIndex].childIndices[0], programGenerationContext);
                rightSideString = recursivlyGenerateStringFor(dag, dag.elements[elementIndex].childIndices[1], programGenerationContext);

                return "(" + leftSideString + operationAsString + rightSideString + ")";
            }
            else if( dag.elements[elementIndex].content.type == DagElementData.EnumType.CONSTPI )
            {
                return callbacks.generateStringForConstantPi();
            }
            else if( dag.elements[elementIndex].content.type == DagElementData.EnumType.PARAM )
            {
                return "(" + dag.elements[elementIndex].content.identifier + ")";
            }
            else if (dag.elements[elementIndex].content.type == DagElementData.EnumType.RETURN )
            {
                string childString;

                childString = recursivlyGenerateStringFor(dag, dag.elements[elementIndex].childIndices[0], programGenerationContext);

                return string.Format("return ({0});\n", childString);
            }
            else
            {
                throw new TranslationException();
            }
        }

        // throws TranslationException
        private static string getIdentifierOfElement(Datastructures.Dag<DagElementData> dag, int elementIndex)
        {
            if (!(dag.elements[elementIndex].content.type == DagElementData.EnumType.IDENTIFIERNAME))
            {
                throw new TranslationException();
            }

            return dag.elements[elementIndex].content.identifier;
        }

        // throws TranslationException
        private static string getStringOfConstInt(Datastructures.Dag<DagElementData> dag, int elementIndex)
        {
            if (!(dag.elements[elementIndex].content.type == DagElementData.EnumType.CONSTINT))
            {
                throw new TranslationException();
            }

            return dag.elements[elementIndex].content.valueInt.ToString();
        }

        private static string getStringOfConstFloat(Datastructures.Dag<DagElementData> dag, int elementIndex)
        {
            if (!(dag.elements[elementIndex].content.type == DagElementData.EnumType.CONSTFLOAT))
            {
                throw new TranslationException();
            }

            return dag.elements[elementIndex].content.valueInt.ToString();
        }

        // throws TranslationException
        private static string getStringOfTypeOf(Datastructures.Dag<DagElementData> dag, int elementIndex)
        {
            if (dag.elements[elementIndex].content.type == DagElementData.EnumType.CONSTINT)
            {
                return "int";
            }
            else if (dag.elements[elementIndex].content.type == DagElementData.EnumType.CONSTFLOAT)
            {
                return "float";
            }

            throw new TranslationException();
        }
    }
}
