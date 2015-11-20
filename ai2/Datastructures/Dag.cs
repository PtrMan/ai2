using System;
using System.Collections.Generic;

namespace Datastructures
{
    class Dag<Type>
    {
        public class Element
        {
            public Element(Type content)
            {
                this.content = content;
            }

            public List<int> childIndices = new List<int>();
            public Type content;
        }

        public void addElement(Element element)
        {
            elements.Add(element);
        }

        public List<Element> elements = new List<Element>();
    }
}
