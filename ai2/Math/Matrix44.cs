namespace Math
{
    class Matrix44
    {
        public Matrix44(float[] array)
        {
            this.array = array;
        }

        public static Matrix44 createIdentity()
        {
            return new Matrix44(new float[]{1.0f, 0.0f, 0.0f, 0.0f,  0.0f, 1.0f, 0.0f, 0.0f,  0.0f, 0.0f, 1.0f, 0.0f,  0.0f, 0.0f, 0.0f, 1.0f});
        }

        public static Matrix44 createRotationX(float cos, float sin)
        {
            return new Matrix44(new float[]{
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f,  cos, -sin, 0.0f,
                0.0f,  sin,  cos, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            });
        }

        public static Matrix44 createRotationY(float cos, float sin)
        {
            return new Matrix44(new float[]{
                cos, 0.0f,  sin, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f,
                -sin, 0.0f,  cos, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            });
        }

        public static Matrix44 createRotationZ(float cos, float sin)
        {
            return new Matrix44(new float[]{
                cos, -sin, 0.0f, 0.0f,
                sin,  cos, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            });
        }

        public static Matrix44 createTranslation(float x, float y, float z)
        {
            Matrix44 result;

            result = Matrix44.createIdentity();
            result.setAt(3, 0, x);
            result.setAt(3, 1, y);
            result.setAt(3, 2, z);

            return result;
        }

        public static Matrix44 createScale(float x, float y, float z)
        {
            Matrix44 result;

            result = Matrix44.createIdentity();
            result.setAt(0, 0, x);
            result.setAt(1, 1, y);
            result.setAt(2, 2, z);

            return result;
        }

        public void mul(Matrix44 other)
        {
            float []result;

            result = new float[16];

            result[0 + 0*4] = Matrix44.vectorDot4(array[0 + 0*4], array[1 + 0*4], array[2 + 0*4], array[3 + 0*4], other.array[0 + 0*4], other.array[0 + 1*4], other.array[0 + 2*4], other.array[0 + 3*4]);
            result[1 + 0*4] = Matrix44.vectorDot4(array[0 + 0*4], array[1 + 0*4], array[2 + 0*4], array[3 + 0*4], other.array[1 + 0*4], other.array[1 + 1*4], other.array[1 + 2*4], other.array[1 + 3*4]);
            result[2 + 0*4] = Matrix44.vectorDot4(array[0 + 0*4], array[1 + 0*4], array[2 + 0*4], array[3 + 0*4], other.array[2 + 0*4], other.array[2 + 1*4], other.array[2 + 2*4], other.array[2 + 3*4]);
            result[3 + 0*4] = Matrix44.vectorDot4(array[0 + 0*4], array[1 + 0*4], array[2 + 0*4], array[3 + 0*4], other.array[3 + 0*4], other.array[3 + 1*4], other.array[3 + 2*4], other.array[3 + 3*4]);

            result[0 + 1*4] = Matrix44.vectorDot4(array[0 + 1*4], array[1 + 1*4], array[2 + 1*4], array[3 + 1*4], other.array[0 + 0*4], other.array[0 + 1*4], other.array[0 + 2*4], other.array[0 + 3*4]);
            result[1 + 1*4] = Matrix44.vectorDot4(array[0 + 1*4], array[1 + 1*4], array[2 + 1*4], array[3 + 1*4], other.array[1 + 0*4], other.array[1 + 1*4], other.array[1 + 2*4], other.array[1 + 3*4]);
            result[2 + 1*4] = Matrix44.vectorDot4(array[0 + 1*4], array[1 + 1*4], array[2 + 1*4], array[3 + 1*4], other.array[2 + 0*4], other.array[2 + 1*4], other.array[2 + 2*4], other.array[2 + 3*4]);
            result[3 + 1*4] = Matrix44.vectorDot4(array[0 + 1*4], array[1 + 1*4], array[2 + 1*4], array[3 + 1*4], other.array[3 + 0*4], other.array[3 + 1*4], other.array[3 + 2*4], other.array[3 + 3*4]);

            result[0 + 2*4] = Matrix44.vectorDot4(array[0 + 2*4], array[1 + 2*4], array[2 + 2*4], array[3 + 2*4], other.array[0 + 0*4], other.array[0 + 1*4], other.array[0 + 2*4], other.array[0 + 3*4]);
            result[1 + 2*4] = Matrix44.vectorDot4(array[0 + 2*4], array[1 + 2*4], array[2 + 2*4], array[3 + 2*4], other.array[1 + 0*4], other.array[1 + 1*4], other.array[1 + 2*4], other.array[1 + 3*4]);
            result[2 + 2*4] = Matrix44.vectorDot4(array[0 + 2*4], array[1 + 2*4], array[2 + 2*4], array[3 + 2*4], other.array[2 + 0*4], other.array[2 + 1*4], other.array[2 + 2*4], other.array[2 + 3*4]);
            result[3 + 2*4] = Matrix44.vectorDot4(array[0 + 2*4], array[1 + 2*4], array[2 + 2*4], array[3 + 2*4], other.array[3 + 0*4], other.array[3 + 1*4], other.array[3 + 2*4], other.array[3 + 3*4]);

            result[0 + 3*4] = Matrix44.vectorDot4(array[0 + 3*4], array[1 + 3*4], array[2 + 3*4], array[3 + 3*4], other.array[0 + 0*4], other.array[0 + 1*4], other.array[0 + 2*4], other.array[0 + 3*4]);
            result[1 + 3*4] = Matrix44.vectorDot4(array[0 + 3*4], array[1 + 3*4], array[2 + 3*4], array[3 + 3*4], other.array[1 + 0*4], other.array[1 + 1*4], other.array[1 + 2*4], other.array[1 + 3*4]);
            result[2 + 3*4] = Matrix44.vectorDot4(array[0 + 3*4], array[1 + 3*4], array[2 + 3*4], array[3 + 3*4], other.array[2 + 0*4], other.array[2 + 1*4], other.array[2 + 2*4], other.array[2 + 3*4]);
            result[3 + 3*4] = Matrix44.vectorDot4(array[0 + 3*4], array[1 + 3*4], array[2 + 3*4], array[3 + 3*4], other.array[3 + 0*4], other.array[3 + 1*4], other.array[3 + 2*4], other.array[3 + 3*4]);

            array = result;
        }

        public Vector3<float> mulVector3f(Vector3<float> other)
        {
            Vector3<float> result;
            
            result = new Vector3<float>(0.0f, 0.0f, 0.0f);
            result.x = Matrix44.vectorDot4(other.x, other.y, other.z, 0.0f,  this.array[0 + 0*4], this.array[1 + 0*4], this.array[2 + 0*4], this.array[3 + 0*4]);
            result.y = Matrix44.vectorDot4(other.x, other.y, other.z, 0.0f,  this.array[0 + 1*4], this.array[1 + 1*4], this.array[2 + 1*4], this.array[3 + 1*4]);
            result.z = Matrix44.vectorDot4(other.x, other.y, other.z, 0.0f,  this.array[0 + 2*4], this.array[1 + 2*4], this.array[2 + 2*4], this.array[3 + 2*4]);

            return result;
        }

        public void setAt(int x, int y, float value)
        {
            System.Diagnostics.Debug.Assert(x >= 0 && x < 4);
            System.Diagnostics.Debug.Assert(y >= 0 && y < 4);

            array[x + y*4] = value;
        }


        public float[] getRawData()
        {
            return array;
        }

        static private float vectorDot4(float A0, float A1, float A2, float A3, float B0, float B1, float B2, float B3)
        {
            return A0*B0 + A1*B1 + A2*B2 + A3*B3;
        }
        
        private float[] array;
    }
}
