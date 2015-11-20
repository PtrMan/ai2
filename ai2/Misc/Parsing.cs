using System;
using System.Collections.Generic;

namespace Misc
{
    class Parsing
    {
        static public bool isLetter(char text)
        {
            return text.ToString().ToLower().IndexOfAny(new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' }) != -1;
        }
    }
}
