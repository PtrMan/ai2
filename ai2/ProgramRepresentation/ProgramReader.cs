using System;
using System.Collections.Generic;

using System.Runtime.Serialization;
using System.IO;
using System.Text.RegularExpressions; 

namespace ProgramRepresentation
{
    class ProgramReader
    {
        [Serializable]
        public class ParsingError : Exception
        {
            // Constructors
            public ParsingError(int line, string description)
                : base("ParsingError")
            {
                this.line = line;
                this.description = description;
            
            }

            // Ensure Exception is Serializable
            protected ParsingError(SerializationInfo info, StreamingContext ctxt)
                : base(info, ctxt)
            { }

            public int line;
            public string description;
        }

        [Serializable]
        public class LinkageError : Exception
        {
            // Constructors
            public LinkageError(string description)
                : base("ParsingError")
            {
                this.description = description;

            }

            // Ensure Exception is Serializable
            protected LinkageError(SerializationInfo info, StreamingContext ctxt)
                : base(info, ctxt)
            { }

            public string description;
        }




        private class DagDataInfo
        {
            public DagDataInfo(string typeAsString, DagElementData.EnumType type, int childIndicesLength)
            {
                this.typeAsString = typeAsString;
                this.type = type;
                this.childIndicesLength = childIndicesLength;
            }

            public DagElementData.EnumType type;
            public int childIndicesLength;
            public string typeAsString;
        }

        private class InstructionLinkageHolder
        {
            public class LinkageHolder
            {
                public enum Type
                {
                    GLOBAL,
                    LOCAL
                }

                public LinkageHolder(Type type, string name)
                {
                    this.type = type;
                    this.name = name;
                }

                public Type type;
                public string name;
                //public int index = -1;
            }


            public InstructionLinkageHolder(LinkageHolder linkageHolder)
            {
                this.linkageHolder = linkageHolder;
            }

            public LinkageHolder linkageHolder;
            public List<LinkageHolder> childLinks = new List<LinkageHolder>();
        }


        public static ProgramRepresentation.Program readProgram(string filename)
        {
            List<string> readLines;
            int lineI;
            Regex introRegex = new Regex("^<(?<type>[lg]) (?<name>[A-Za-z0-9]+)>");
            Regex labelRegex = new Regex("^(?<type>[lg]) (?<name>[A-Za-z0-9]+)");
            List<InstructionLinkageHolder> instructionLinkageHolders = new List<InstructionLinkageHolder>();
            ProgramRepresentation.Program resultProgram = new Program();

            readLines = Misc.TextFile.readLinesFromFile(filename);

            for( lineI = 0; lineI < readLines.Count; lineI++ )
            {
                string currentLine;
                Match introRegexMatch;
                string labelType;
                string labelName;
                int remainingLines;
                string instructionTypeString;
                InstructionLinkageHolder.LinkageHolder.Type instructionLinkageHolderType;

                remainingLines = readLines.Count - lineI - 1;

                currentLine = readLines[lineI];

                if( currentLine.Length >= 2 && currentLine.Substring(0, 2) == "//" )
                {
                    continue;
                }
                if( currentLine.Length == 0 )
                {
                    continue;
                }

                introRegexMatch = introRegex.Match(currentLine);

                if( !introRegexMatch.Success )
                {
                    throw new ParsingError(lineI+1, "No valid label/instruction!");
                }

                labelType = introRegexMatch.Groups["type"].Value;
                labelName = introRegexMatch.Groups["name"].Value;

                if( labelType == "l" )
                {
                    // local
                    instructionLinkageHolderType = InstructionLinkageHolder.LinkageHolder.Type.LOCAL;
                }
                else if( labelType == "g" )
                {
                    // global
                    instructionLinkageHolderType = InstructionLinkageHolder.LinkageHolder.Type.GLOBAL;
                }
                else
                {
                    throw new ParsingError(lineI+1, "label type must be either (g)lobal or (l)ocal!");
                }

                if( remainingLines == 0 )
                {
                    throw new ParsingError(lineI+1, "too less lines!");
                }

                instructionTypeString = readLines[lineI+1];

                DagDataInfo dagDataInfo = getDagDataInfoByTypeString(lineI + 1, instructionTypeString);
                bool remainingLengthEnought = checkRemainingLength(dagDataInfo, remainingLines);

                if( !remainingLengthEnought )
                {
                    throw new ParsingError(lineI + 1, "too few lines!");
                }

                lineI++;

                InstructionLinkageHolder instructionLinkageHolderForThisInstruction;


                instructionLinkageHolderForThisInstruction = new InstructionLinkageHolder(new InstructionLinkageHolder.LinkageHolder(instructionLinkageHolderType, labelName));

                instructionLinkageHolders.Add(instructionLinkageHolderForThisInstruction);

                int childI;

                for( childI = 0; childI < dagDataInfo.childIndicesLength; childI++ )
                {
                    string childString;
                    Match labelRegexMatch;
                    string childLabelType, childlabelName;
                    InstructionLinkageHolder.LinkageHolder.Type childInstructionLinkageHolderType;
                    InstructionLinkageHolder.LinkageHolder childLinkage;

                    childString = readLines[lineI + 1 + childI];
                    labelRegexMatch = labelRegex.Match(childString);

                    if( !labelRegexMatch.Success )
                    {
                        throw new ParsingError(lineI + 1 + childI, "can't parse label");
                    }

                    childLabelType = labelRegexMatch.Groups["type"].Value;
                    childlabelName = labelRegexMatch.Groups["name"].Value;

                    if( childLabelType == "l" )
                    {
                        childInstructionLinkageHolderType = InstructionLinkageHolder.LinkageHolder.Type.LOCAL;
                    }
                    else if( childLabelType == "g" )
                    {
                        childInstructionLinkageHolderType = InstructionLinkageHolder.LinkageHolder.Type.GLOBAL;
                    }
                    else
                    {
                        throw new ParsingError(lineI + 1 + childI, "invalid type");
                    }

                    childLinkage = new InstructionLinkageHolder.LinkageHolder(childInstructionLinkageHolderType, childlabelName);

                    instructionLinkageHolderForThisInstruction.childLinks.Add(childLinkage);
                }

                lineI += dagDataInfo.childIndicesLength + 1;


                ProgramRepresentation.DagElementData newDagElementData;

                newDagElementData = new DagElementData();
                newDagElementData.type = dagDataInfo.type;

                string identifierName;
                string valueString;



                switch( dagDataInfo.type )
                {
                    case DagElementData.EnumType.IDENTIFIERNAME:
                    case DagElementData.EnumType.ARRAYREAD2D:
                    case DagElementData.EnumType.ARRAYREAD1D:
                    case DagElementData.EnumType.PARAM:
                    if( lineI + 1 > readLines.Count )
                    {
                        throw new ParsingError(lineI + 1, "too few lines!");
                    }
                    identifierName = readLines[lineI];
                    lineI++;

                    newDagElementData.identifier = identifierName;

                    break;

                    case DagElementData.EnumType.CONSTINT:
                    if (lineI + 1 > readLines.Count)
                    {
                        throw new ParsingError(lineI + 1, "too few lines!");
                    }
                    valueString = readLines[lineI];
                    lineI++;

                    newDagElementData.valueInt = Convert.ToInt32(valueString);

                    break;



                    case DagElementData.EnumType.CONSTFLOAT:
                    if (lineI + 1 > readLines.Count)
                    {
                        throw new ParsingError(lineI + 1, "too few lines!");
                    }
                    valueString = readLines[lineI];
                    lineI++;

                    newDagElementData.valueFloat = (float)Convert.ToDouble(valueString);

                    break;
                }


                // add instruction

                resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(newDagElementData));
            }

            // linkage
            doLinkage(resultProgram, instructionLinkageHolders);

            return resultProgram;
        }

        private static void doLinkage(ProgramRepresentation.Program program, List<InstructionLinkageHolder> instructionLinkageHolders)
        {
            int instructionI;
            
            System.Diagnostics.Debug.Assert(program.dag.elements.Count == instructionLinkageHolders.Count);

            for( instructionI = 0; instructionI < instructionLinkageHolders.Count; instructionI++ )
            {
                foreach( InstructionLinkageHolder.LinkageHolder iterationLinkage in instructionLinkageHolders[instructionI].childLinks )
                {
                    if( iterationLinkage.type == InstructionLinkageHolder.LinkageHolder.Type.LOCAL )
                    {
                        // local

                        int linkedIndex;

                        linkedIndex = getIndexOfLocalLabel(instructionLinkageHolders, iterationLinkage.name);

                        program.dag.elements[instructionI].childIndices.Add(linkedIndex);
                    }
                    else
                    {
                        // global

                        // TODO
                        throw new LinkageError("TODO");

                        //program.dag.elements[instructionI].childIndices.Add(linkedIndex);
                    }
                }
            }
        }

        private static int getIndexOfLocalLabel(List<InstructionLinkageHolder> instructionLinkageHolders, string labelname)
        {
            int i;

            for( i = 0; i < instructionLinkageHolders.Count; i++ )
            {
                if( instructionLinkageHolders[i].linkageHolder.name == labelname && instructionLinkageHolders[i].linkageHolder.type == InstructionLinkageHolder.LinkageHolder.Type.LOCAL )
                {
                    return i;
                }
            }

            throw new LinkageError(labelname + " not found!");
        }

        private static bool checkRemainingLength(DagDataInfo dagDataInfo, int remainingLines)
        {
            if( dagDataInfo.childIndicesLength == -1 )
            {
                return true;
            }

            return dagDataInfo.childIndicesLength <= remainingLines;
        }

        /*
        private static DagDataInfo getDagDataInfoByType(int line, DagElementData.EnumType type)
        {
            int i;

            for( i = 0; i < lengthInfos.Length; i++ )
            {
                if( lengthInfos[i].type == type )
                {
                    return lengthInfos[i];
                }
            }

            throw new ParsingError(line, "Type is invalid!");
        }
         */

        private static DagDataInfo getDagDataInfoByTypeString(int line, string type)
        {
            int i;

            for (i = 0; i < lengthInfos.Length; i++)
            {
                if (lengthInfos[i].typeAsString == type)
                {
                    return lengthInfos[i];
                }
            }

            throw new ParsingError(line, "Type is invalid!");
        }

        private static DagDataInfo[] lengthInfos = {
            new DagDataInfo("CONSTLOOP", DagElementData.EnumType.CONSTLOOP, 4),
            new DagDataInfo("IDENTIFIERNAME", DagElementData.EnumType.IDENTIFIERNAME, 0),
            new DagDataInfo("CONSTINT", DagElementData.EnumType.CONSTINT, 0),
            new DagDataInfo("SEQUENCE", DagElementData.EnumType.SEQUENCE, -1),
            new DagDataInfo("ALLOCATESET", DagElementData.EnumType.ALLOCATESET, 2),
            new DagDataInfo("CONSTFLOAT", DagElementData.EnumType.CONSTFLOAT, 0),
            new DagDataInfo("ADDASSIGN", DagElementData.EnumType.ADDASSIGN, 2),
            new DagDataInfo("MUL", DagElementData.EnumType.MUL, 2),
            new DagDataInfo("READINPUTAT2D", DagElementData.EnumType.READINPUTAT2D, 2),
            new DagDataInfo("ADD", DagElementData.EnumType.ADD, 2),
            new DagDataInfo("ARRAYREAD2D", DagElementData.EnumType.ARRAYREAD2D, 2),
            new DagDataInfo("WRITERESULT", DagElementData.EnumType.WRITERESULT, 1),
            new DagDataInfo("ASSIGNMENTINT", DagElementData.EnumType.ASSIGNMENTINT, 2),
            new DagDataInfo("ARRAYREAD1D", DagElementData.EnumType.ARRAYREAD1D, 1),
            new DagDataInfo("DIV", DagElementData.EnumType.DIV, 2),
            new DagDataInfo("NULL", DagElementData.EnumType.NULL, 1),
            new DagDataInfo("PARAM", DagElementData.EnumType.PARAM, 0),
            new DagDataInfo("CONSTPI", DagElementData.EnumType.CONSTPI, 0),
            new DagDataInfo("SUB", DagElementData.EnumType.SUB, 2),
            new DagDataInfo("RETURN", DagElementData.EnumType.RETURN, 1)
        };
    }
}
