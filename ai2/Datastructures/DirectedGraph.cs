using System;
using System.Collections.Generic;

namespace Datastructures
{
    class DirectedGraph<Type, EdgeWeighttype>
    {
        public class Element
        {
            public Element(Type content)
            {
                this.content = content;
            }

            public List<int> parentIndices = new List<int>();
            public List<int> childIndices = new List<int>();
            public Type content;

            public List<EdgeWeighttype> childWeights = new List<EdgeWeighttype>();
        }

        public void addElement(Element element)
        {
            elements.Add(element);
        }

        public EdgeWeighttype getEdgeWeight(int from, int to)
        {
            Element selectedElement;
            int childI;

            System.Diagnostics.Debug.Assert(from >= 0);
            System.Diagnostics.Debug.Assert(to >= 0);

            selectedElement = elements[from];

            for (childI = 0; childI < selectedElement.childWeights.Count; childI++ )
            {
                if( selectedElement.childIndices[childI] == to )
                {
                    return selectedElement.childWeights[childI];
                }
            }

            System.Diagnostics.Debug.Assert(false, "Edge weight not found, shouldn't happen!");
            return default(EdgeWeighttype);
        }

        public List<Element> elements = new List<Element>();
        
    }
}
