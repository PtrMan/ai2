using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaffolds.Graph
{
    class ExtractLineScaffoldInstanceData : GraphScaffoldInstanceData
    {
        public Misc.Vector2<float> startVertexPositionA;
        public Misc.Vector2<float> startVertexPositionB;



        // temporary
        public int startVertexIndexA;
        public int startVertexIndexB;

        // vertex index of the last point of the "trace"
        public int lastVertexIndex;

        

        public enum EnumStartPositionState
        {
            NOTDECIDED,
            A,
            B
        }

        // indicates which start position was chosen as the start position of the line, depends on 2nd vertex/edge combination
        public EnumStartPositionState startPositionState = EnumStartPositionState.NOTDECIDED;

        public Misc.Vector2<float> getFirstPosition()
        {
            System.Diagnostics.Debug.Assert(startPositionState != EnumStartPositionState.NOTDECIDED);

            if( startPositionState == EnumStartPositionState.A )
            {
                return startVertexPositionA;
            }
            else
            {
                return startVertexPositionB;
            }
        }
    }
}
