using System;

namespace Math
{
    public class NativeMatrix
    {
        public static Type[,] multiply<Type>(Type[,] a, Type[,] b)
        {
            int x, y;
            Type[,] result;

            System.Diagnostics.Debug.Assert(a.GetLength(0) == b.GetLength(1));
            System.Diagnostics.Debug.Assert(a.GetLength(1) == b.GetLength(0));

            result = new Type[b.GetLength(0), a.GetLength(1)];

            for( y = 0; y < a.GetLength(1) ; y++ )
            {
                for( x = 0; x < b.GetLength(0); x++ )
                {
                    result[x, y] = multiplyVectors<Type>(ref a, ref b, x, y);
                }
            }

            return result;
        }

        private static Type multiplyVectors<Type>(ref Type[,] a, ref Type[,] b, int column, int row)
        {
            int i;
            dynamic result;

            result = default(Type);

            for( i = 0; i < a.GetLength(0); i++ )
            {
                dynamic valueA;
                dynamic valueB;

                valueA = a[i, row];
                valueB = b[column, i];

                dynamic temp = valueA * valueB;

                result += temp;
            }

            return result;
        }
    }
}
