using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramRepresentation
{
    class Program
    {
        public Datastructures.Dag<DagElementData> dag = new Datastructures.Dag<DagElementData>();

        public enum EnumType
        {
            PARALLEL, // is a parallel program, usefull for C++/OpenCL synthesis
        }

        public EnumType type;

        // just for testing in here
        static public Program createProgramForParallelRadialKernel()
        {
            Program resultProgram;

            /*
             * algorithm:
             * 
             * result@ = 0.0
             * for(x = 0, x < 4, x++)
             *    for(y = 0, y < 4, y++)
             *       result += (readInputAt(posx + x, posy + y) * arr[x, y])
             * 
             * write result@
             * 
             * [ 0] entry sequence
             * [ 1] outer loop
             * [ 2] outer x/inner y loop begin
             * [ 3] outer x/inner y loop end
             * [ 4] outer x loop variable
             * [ 5] inner loop
             * [ 6] inner y loop variable
             * [ 7] allocateset for result
             * [ 8] result name
             * [ 9] +=
             * [10] *
             * [11] readInputAt
             * [12] +
             * [13] posx
             * [14] +
             * [15] posy
             * [16] array read arr
             * [17] result
             */

            resultProgram = new Program();

            resultProgram.type = EnumType.PARALLEL;

            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [ 0]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [ 1]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [ 2]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [ 3]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [ 4]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [ 5]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [ 6]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [ 7]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [ 8]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [ 9]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [10]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [11]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [12]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [13]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [14]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [15]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [16]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [17]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [18]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [19]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [20]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [21]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [22]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [23]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [24]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [25]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [26]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [27]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [28]
            resultProgram.dag.addElement(new Datastructures.Dag<DagElementData>.Element(new DagElementData())); // [29]

            resultProgram.dag.elements[0].content.type = DagElementData.EnumType.SEQUENCE;
            resultProgram.dag.elements[0].childIndices.Add(19);
            resultProgram.dag.elements[0].childIndices.Add(26);
            resultProgram.dag.elements[0].childIndices.Add(7);
            resultProgram.dag.elements[0].childIndices.Add(1);
            resultProgram.dag.elements[0].childIndices.Add(17);

            resultProgram.dag.elements[1].content.type = DagElementData.EnumType.CONSTLOOP;
            resultProgram.dag.elements[1].childIndices.Add(2);
            resultProgram.dag.elements[1].childIndices.Add(3);
            resultProgram.dag.elements[1].childIndices.Add(4);
            resultProgram.dag.elements[1].childIndices.Add(5);

            resultProgram.dag.elements[2].content.type = DagElementData.EnumType.CONSTINT;
            resultProgram.dag.elements[2].content.valueInt = 0;

            resultProgram.dag.elements[3].content.type = DagElementData.EnumType.CONSTINT;
            resultProgram.dag.elements[3].content.valueInt = 3;

            resultProgram.dag.elements[4].content.type = DagElementData.EnumType.IDENTIFIERNAME;
            resultProgram.dag.elements[4].content.identifier = "x";

            resultProgram.dag.elements[5].content.type = DagElementData.EnumType.CONSTLOOP;
            resultProgram.dag.elements[5].childIndices.Add(2);
            resultProgram.dag.elements[5].childIndices.Add(3);
            resultProgram.dag.elements[5].childIndices.Add(6);
            resultProgram.dag.elements[5].childIndices.Add(9);

            resultProgram.dag.elements[6].content.type = DagElementData.EnumType.IDENTIFIERNAME;
            resultProgram.dag.elements[6].content.identifier = "y";

            resultProgram.dag.elements[7].content.type = DagElementData.EnumType.ALLOCATESET;
            resultProgram.dag.elements[7].childIndices.Add(18);
            resultProgram.dag.elements[7].childIndices.Add(8);

            resultProgram.dag.elements[8].content.type = DagElementData.EnumType.CONSTFLOAT;
            resultProgram.dag.elements[8].content.valueFloat = 0.0f;

            resultProgram.dag.elements[9].content.type = DagElementData.EnumType.ADDASSIGN;
            resultProgram.dag.elements[9].childIndices.Add(18);
            resultProgram.dag.elements[9].childIndices.Add(10);

            resultProgram.dag.elements[10].content.type = DagElementData.EnumType.MUL;
            resultProgram.dag.elements[10].childIndices.Add(11);
            resultProgram.dag.elements[10].childIndices.Add(16);

            resultProgram.dag.elements[11].content.type = DagElementData.EnumType.READINPUTAT2D;
            resultProgram.dag.elements[11].childIndices.Add(12);
            resultProgram.dag.elements[11].childIndices.Add(14);
            
            resultProgram.dag.elements[12].content.type = DagElementData.EnumType.ADD;
            resultProgram.dag.elements[12].childIndices.Add(4);
            resultProgram.dag.elements[12].childIndices.Add(13);

            resultProgram.dag.elements[13].content.type = DagElementData.EnumType.IDENTIFIERNAME;
            resultProgram.dag.elements[13].content.identifier = "posx";
            
            resultProgram.dag.elements[14].content.type = DagElementData.EnumType.ADD;
            resultProgram.dag.elements[14].childIndices.Add(6);
            resultProgram.dag.elements[14].childIndices.Add(15);

            resultProgram.dag.elements[15].content.type = DagElementData.EnumType.IDENTIFIERNAME;
            resultProgram.dag.elements[15].content.identifier = "posy";

            resultProgram.dag.elements[16].content.type = DagElementData.EnumType.ARRAYREAD2D;
            resultProgram.dag.elements[16].content.identifier = "kernelArray";
            resultProgram.dag.elements[16].childIndices.Add(4);
            resultProgram.dag.elements[16].childIndices.Add(6);

            resultProgram.dag.elements[17].content.type = DagElementData.EnumType.WRITERESULT;
            resultProgram.dag.elements[17].childIndices.Add(18);

            resultProgram.dag.elements[18].content.type = DagElementData.EnumType.IDENTIFIERNAME;
            resultProgram.dag.elements[18].content.identifier = "result";


            resultProgram.dag.elements[19].content.type = DagElementData.EnumType.ASSIGNMENTINT;
            resultProgram.dag.elements[19].childIndices.Add(13);
            resultProgram.dag.elements[19].childIndices.Add(20);

            resultProgram.dag.elements[20].content.type = DagElementData.EnumType.ARRAYREAD1D;
            resultProgram.dag.elements[20].content.identifier = "positions";
            resultProgram.dag.elements[20].childIndices.Add(21);

            resultProgram.dag.elements[21].content.type = DagElementData.EnumType.ADD;
            resultProgram.dag.elements[21].childIndices.Add(25);
            resultProgram.dag.elements[21].childIndices.Add(22);

            resultProgram.dag.elements[22].content.type = DagElementData.EnumType.MUL;
            resultProgram.dag.elements[22].childIndices.Add(23);
            resultProgram.dag.elements[22].childIndices.Add(24);

            resultProgram.dag.elements[23].content.type = DagElementData.EnumType.IDENTIFIERNAME;
            resultProgram.dag.elements[23].content.identifier = "indexX";

            resultProgram.dag.elements[24].content.type = DagElementData.EnumType.CONSTINT;
            resultProgram.dag.elements[24].content.valueInt = 2;

            resultProgram.dag.elements[25].content.type = DagElementData.EnumType.CONSTINT;
            resultProgram.dag.elements[25].content.valueInt = 0;



            resultProgram.dag.elements[26].content.type = DagElementData.EnumType.ASSIGNMENTINT;
            resultProgram.dag.elements[26].childIndices.Add(15);
            resultProgram.dag.elements[26].childIndices.Add(27);

            resultProgram.dag.elements[27].content.type = DagElementData.EnumType.ARRAYREAD1D;
            resultProgram.dag.elements[27].content.identifier = "positions";
            resultProgram.dag.elements[27].childIndices.Add(28);

            resultProgram.dag.elements[28].content.type = DagElementData.EnumType.ADD;
            resultProgram.dag.elements[28].childIndices.Add(29);
            resultProgram.dag.elements[28].childIndices.Add(22);

            resultProgram.dag.elements[29].content.type = DagElementData.EnumType.CONSTINT;
            resultProgram.dag.elements[29].content.valueInt = 1;

            


            return resultProgram;
        }
    }
}
