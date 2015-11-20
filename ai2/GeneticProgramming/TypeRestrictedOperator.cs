using System;
using System.Collections.Generic;

namespace GeneticProgramming
{
    abstract class TypeRestrictedOperator
    {
        public enum EnumResult
        {
            FAILED,
            OK
        }

        /**
         * 
         * \param globals in globals are the global variables for the genetic algorithm stored
         *        they can only be changed from the outside, but they can hold any information the genetic algorithm will/can need
         *        for example
         *         * current Index of Something
         *         * current Frame
         *         * ...
         * 
         */
        public abstract void call(
            List<Datastructures.Variadic> parameters,
            List<Datastructures.Variadic> globals,
            out Datastructures.Variadic result, out EnumResult resultCode
        );

        public abstract void addOperatorAsParameter(TypeRestrictedOperator parameter);

        public abstract TypeRestrictedOperator clone();

        public abstract string getTypeAsString();

        public List<GeneticProgramming.TypeRestrictedOperator> callableOperators = new List<GeneticProgramming.TypeRestrictedOperator>();
    }
}
