using System;
using System.Collections.Generic;

namespace Math
{
    class DynamicVector<Type>
    {
        public Type[] array;

        public DynamicVector(int arraySize)
        {
            array = new Type[arraySize];
        }

        public Type this[int i]
        {
            get
            {
                return array[i];
            }
            set
            {
                array[i] = value;
            }
        }

        public static bool operator !=(DynamicVector<Type> a, DynamicVector<Type> b)
        {
            return !(a == b);
        }
        // is this correct?
        public static bool operator ==(DynamicVector<Type> a, DynamicVector<Type> b)
        {
            int i;
            
            System.Diagnostics.Debug.Assert(a.array.Length == b.array.Length);

            for( i = 0; i < a.array.Length; i++ )
            {
                if( !a[i].Equals(b[i]) )
                {
                    return false;
                }
            }

            return true;
        }
    }
}
