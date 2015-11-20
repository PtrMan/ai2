using System;
using System.Collections.Generic;

namespace ProgramRepresentation
{
    class CsGenerator
    {
        private class Callbacks : ProgramGenerator.IProgramGenerationCallbacks
        {
            public string generateStringForConstantPi()
            {
                return "(float)System.Math.PI";
            }
        }

        public static string generateSource(Program program, int inputMapSize, Dictionary<string, int> widthOfArrays)
        {
            ProgramRepresentation.ProgramGenerator programGenerator;

            programGenerator = new ProgramGenerator(new Callbacks());

            return programGenerator.generateSource(program, inputMapSize, widthOfArrays);
        }
    }
}
