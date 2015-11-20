using System;

using Misc;

namespace Algorithms
{
    class TriangleDrawer
    {
        public static void drawTriangle(Vector2<uint> a, Vector2<uint> b, Vector2<uint> c, Map2d<bool> image)
        {
            Vector2<uint> drawStart;
            Vector2<uint> drawEnd;
            uint x;
            uint y;

            // small hack
            drawStart = Vector2<uint>.min(a, b, c, c);
            drawEnd = Vector2<uint>.max(a, b, c, c);

            for( y = drawStart.y; y < drawEnd.y; y++ )
            {
                for( x = drawStart.x; x < drawEnd.x; x++ )
                {
                    Vector2<float> aAsFloat, bAsFloat, cAsFloat, point;

                    aAsFloat = new Vector2<float>();
                    bAsFloat = new Vector2<float>();
                    cAsFloat = new Vector2<float>();
                    point = new Vector2<float>();

                    aAsFloat.x = (float)a.x;
                    aAsFloat.y = (float)a.y;

                    bAsFloat.x = (float)b.x;
                    bAsFloat.y = (float)b.y;

                    cAsFloat.x = (float)c.x;
                    cAsFloat.y = (float)c.y;

                    point.x = (float)x;
                    point.y = (float)y;

                    if( checkIfItTriangle(aAsFloat, bAsFloat, cAsFloat, point) )
                    {
                        image.writeAt((int)x, (int)y, true);
                    }
                    else
                    {
                        image.writeAt((int)x, (int)y, false);
                    }
                }
            }
        }

        private static bool checkIfItTriangle(Vector2<float> a, Vector2<float> b, Vector2<float> c, Vector2<float> point)
        {
            Vector2<float> diffA, diffB, diffC;
            float dotAB, dotAC, dotBC;
            float angleSum;

            diffA = a - point;
            diffB = b - point;
            diffC = c - point;

            if( diffA.magnitude() == 0.0f || diffB.magnitude() == 0.0f || diffC.magnitude() == 0.0f )
            {
                return false;
            }

            diffA.normalize();
            diffB.normalize();
            diffC.normalize();

            dotAB = diffA.dot(diffB);
            dotAC = diffA.dot(diffC);
            dotBC = diffB.dot(diffC);

            angleSum = (float)System.Math.Acos(dotAB) + (float)System.Math.Acos(dotAC) + (float)System.Math.Acos(dotBC);

            return angleSum > 2.0f * System.Math.PI - /*epsilon*/0.01f;
        }
    }
}
