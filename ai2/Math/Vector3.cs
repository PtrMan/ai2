using System;
using System.Collections.Generic;

namespace Math
{
    class Vector3<Type>
    {
        public Vector3(Type x, Type y, Type z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Type x;
        public Type y;
        public Type z;
    }
}
