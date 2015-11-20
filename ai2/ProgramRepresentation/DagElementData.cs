using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramRepresentation
{
    class DagElementData
    {
        /**
         * CONSTLOOP
         * dag childrens are
         * [0] constant begin value
         * [1] constant end value
         * [2] iterator name
         * [3] body
         * 
         * IDENTIFIERNAME
         * names a variable which is used for anything, depending on using dag element
         * .identifier is the identifier
         * 
         * CONSTINT
         * is the constant integer value
         * .valueInt is the value
         * 
         * SEQUENCE
         * is a sequence of instructions which are executed after each other
         * 
         * ALLOCATESET
         * allocates and sets a variable
         * [0] variable/identifier
         * [1] value
         * 
         * dag elements
         * [0] value
         * 
         * CONSTFLOAT
         * is the constant float value
         * .valueFloat is the value
         * 
         * ADDASSIGN
         * [0] left
         * [1] right
         * 
         * MUL
         * [0] left
         * [1] right
         * 
         * READINPUTAT2D
         * [0] x
         * [1] y
         * 
         * ADD
         * [0] left
         * [1] right
         * 
         * ARRAYREAD2D
         * .identifier name of the array
         * [0] x
         * [1] y
         * 
         * WRITERESULT
         * actually used only in parallel code
         * [0] identfier/value
         * 
         * ASSIGNMENTINT
         * [0] identifier
         * [1] value tree
         * 
         * ARRAYREAD1D
         * .identifier name of the array
         * [0] x
         * 
         * DIV
         * [0] left
         * [1] right
         * 
         * NULL
         * [0] children
         * 
         * PARAM
         * .identifier name of the parameter
         * 
         * CONSTPI
         * 
         * SUB
         * [0]
         * [1]
         * 
         * RETURN
         * [0]
         * 
         * 
         * FSCOPE
         * functional scope
         * .valueInt is the index of the called function/type
         * 
         * childs are the arguments
         * 
         * FARRAY
         * functional array
         * 
         * childs are the elements
         * 
         * CONSTSTRING
         * .valueString content of the string
         * 
         * CONSTBOOL
         * .valueBool
         * 
         * FSET
         */

        public enum EnumType
        {
            CONSTLOOP,
            IDENTIFIERNAME,
            CONSTINT,
            SEQUENCE,
            ALLOCATESET,
            CONSTFLOAT,
            ADDASSIGN,
            MUL,
            READINPUTAT2D,
            ADD,
            ARRAYREAD2D,
            WRITERESULT,
            ASSIGNMENTINT,
            ARRAYREAD1D,
            DIV,
            NULL,
            PARAM,
            CONSTPI,
            SUB,
            RETURN,
            FSCOPE,
            FARRAY,
            CONSTSTRING,
            CONSTBOOL,
        }

        public EnumType type;

        public string identifier;

        public int valueInt;
        public float valueFloat;
        public string valueString;
        public bool valueBool;
    }
}
