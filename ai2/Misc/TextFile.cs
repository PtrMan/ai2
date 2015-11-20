using System;
using System.Collections.Generic;

namespace Misc
{
    class TextFile
    {
        public static List<string> readLinesFromFile(string filename)
        {
            System.IO.StreamReader file;
            string line;
            List<string> resultLines;

            resultLines = new List<string>();

            file = new System.IO.StreamReader(filename);
            while ((line = file.ReadLine()) != null)
            {
                resultLines.Add(line);
            }

            return resultLines;
        }
    }
}
