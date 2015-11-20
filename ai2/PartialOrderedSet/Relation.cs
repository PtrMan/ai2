using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartialOrderedSet
{
    class Relation
    {
        public Relation(int sourceIndex, int destinationIndex)
        {
            this.sourceIndex = sourceIndex;
            this.destinationIndex = destinationIndex;
        }

        public int destinationIndex;
        public int sourceIndex;
    }
}
