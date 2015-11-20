using System;

namespace Algorithm.Visual
{
    class EdgeDetector
    {
        // algorithm from http://www.cse.unr.edu/~bebis/CS791E/Notes/EdgeDetection.pdf
        static public Map2d<bool> detectEdges(Map2d<float> grayImage)
        {
            /*
            Map2d<bool> result;
            int x, y;

            result = new Map2d<bool>(grayImage.getWidth(), grayImage.getLength());

            for( y = 1; y < grayImage.getLength() - 1; y++ )
            {
                for( x = 1; x < grayImage.getWidth() - 1; x++ )
                {
                    float diffX, diffY;

                    diffX = -0.5f * grayImage.readAt(x - 1, y) + 0.5f * grayImage.readAt(x + 1, y);
                    diffY = -0.5f * grayImage.readAt(x, y - 1) + 0.5f * grayImage.readAt(x, y + 1);

                    if( Math.Abs(diffX) > 0.09f || Math.Abs(diffY) > 0.09f )
                    {
                        result.writeAt(x, y, true);
                    }
                }
            }
             */

            Map2d<bool> result;
            int x, y;

            float[] row0;
            float[] row1;
            float[] row2;

            row0 = new float[3];
            row1 = new float[3];
            row2 = new float[3];

            result = new Map2d<bool>(grayImage.getWidth(), grayImage.getLength());

            for (y = 1; y < grayImage.getLength() - 1; y++)
            {
                row0[0] = grayImage.readAt(0, y - 1);
                row0[1] = grayImage.readAt(1, y - 1);
                row0[2] = grayImage.readAt(2, y - 1);

                row1[0] = grayImage.readAt(0, y);
                row1[1] = grayImage.readAt(1, y);
                row1[2] = grayImage.readAt(2, y);

                row2[0] = grayImage.readAt(0, y + 1);
                row2[1] = grayImage.readAt(1, y + 1);
                row2[2] = grayImage.readAt(2, y + 1);

                for (x = 1; x < grayImage.getWidth() - 1; x++)
                {
                    // Sobel edge detector

                    float m00, m10, m20,
                          m01, m11, m21,
                          m02, m12, m22;


                    float mx00, mx10, mx20,
                          mx01, mx11, mx21,
                          mx02, mx12, mx22;


                    float my00, my10, my20,
                          my01, my11, my21,
                          my02, my12, my22;

                    m00 = row0[0]; // grayImage.readAt(x - 1, y - 1);
                    m10 = row0[1]; //grayImage.readAt(x, y - 1);
                    m20 = row0[2]; //grayImage.readAt(x + 1, y - 1);

                    m01 = row1[0];//grayImage.readAt(x - 1, y);
                    m11 = row1[1];//grayImage.readAt(x, y);
                    m21 = row1[2];// grayImage.readAt(x + 1, y);

                    m02 = row2[0]; //grayImage.readAt(x - 1, y + 1);
                    m12 = row2[1];//grayImage.readAt(x, y + 1);
                    m22 = row2[2];//grayImage.readAt(x + 1, y + 1);



                    mx00 = -1.0f;
                    mx10 = 0.0f;
                    mx20 = 1.0f;

                    mx01 = -2.0f;
                    mx11 = 0.0f;
                    mx21 = 2.0f;

                    mx02 = -1.0f;
                    mx12 = 0.0f;
                    mx22 = 1.0f;


                    my00 = -1.0f;
                    my10 = -2.0f;
                    my20 = -1.0f;

                    my01 = 0.0f;
                    my11 = 0.0f;
                    my21 = 0.0f;

                    my02 = 1.0f;
                    my12 = 2.0f;
                    my22 = 1.0f;


                    float resultX = m00 * mx00 + m10 * mx10 + m20 * mx20 +
                                    m01 * mx01 + m11 * mx11 + m21 * mx21 +
                                    m02 * mx02 + m12 * mx12 + m22 * mx22;

                    float resultY = m00 * my00 + m10 * my10 + m20 * my20 +
                                    m01 * my01 + m11 * my11 + m21 * my21 +
                                    m02 * my02 + m12 * my12 + m22 * my22;



                    // 0.9 makes too many (possible usable fine detail) egdes
                    if( System.Math.Abs(resultX) > 1.15f || System.Math.Abs(resultY) > 1.15f )
                    {
                        result.writeAt(x, y, true);
                    }


                    // shift temporary array
                    row0[0] = row0[1];
                    row0[1] = row0[2];

                    row1[0] = row1[1];
                    row1[1] = row1[2];

                    row2[0] = row2[1];
                    row2[1] = row2[2];




                    // push in now pixels
                    row0[2] = grayImage.readAt(x + 1, y - 1);
                    row1[2] = grayImage.readAt(x + 1, y);
                    row2[2] = grayImage.readAt(x + 1, y + 1);


                }
            }

            return result;
        }

        static public Map2d<float> detectEdgesFloat(Map2d<float> grayImage)
        {
            /*
            Map2d<bool> result;
            int x, y;

            result = new Map2d<bool>(grayImage.getWidth(), grayImage.getLength());

            for( y = 1; y < grayImage.getLength() - 1; y++ )
            {
                for( x = 1; x < grayImage.getWidth() - 1; x++ )
                {
                    float diffX, diffY;

                    diffX = -0.5f * grayImage.readAt(x - 1, y) + 0.5f * grayImage.readAt(x + 1, y);
                    diffY = -0.5f * grayImage.readAt(x, y - 1) + 0.5f * grayImage.readAt(x, y + 1);

                    if( Math.Abs(diffX) > 0.09f || Math.Abs(diffY) > 0.09f )
                    {
                        result.writeAt(x, y, true);
                    }
                }
            }
             */

            Map2d<float> result;
            int x, y;

            float[] row0;
            float[] row1;
            float[] row2;

            row0 = new float[3];
            row1 = new float[3];
            row2 = new float[3];

            result = new Map2d<float>(grayImage.getWidth(), grayImage.getLength());

            for (y = 1; y < grayImage.getLength() - 1; y++)
            {
                row0[0] = grayImage.readAt(0, y - 1);
                row0[1] = grayImage.readAt(1, y - 1);
                row0[2] = grayImage.readAt(2, y - 1);

                row1[0] = grayImage.readAt(0, y);
                row1[1] = grayImage.readAt(1, y);
                row1[2] = grayImage.readAt(2, y);

                row2[0] = grayImage.readAt(0, y + 1);
                row2[1] = grayImage.readAt(1, y + 1);
                row2[2] = grayImage.readAt(2, y + 1);

                for (x = 1; x < grayImage.getWidth() - 1; x++)
                {
                    // Sobel edge detector

                    float m00, m10, m20,
                          m01, m11, m21,
                          m02, m12, m22;


                    float mx00, mx10, mx20,
                          mx01, mx11, mx21,
                          mx02, mx12, mx22;


                    float my00, my10, my20,
                          my01, my11, my21,
                          my02, my12, my22;

                    m00 = row0[0]; // grayImage.readAt(x - 1, y - 1);
                    m10 = row0[1]; //grayImage.readAt(x, y - 1);
                    m20 = row0[2]; //grayImage.readAt(x + 1, y - 1);

                    m01 = row1[0];//grayImage.readAt(x - 1, y);
                    m11 = row1[1];//grayImage.readAt(x, y);
                    m21 = row1[2];// grayImage.readAt(x + 1, y);

                    m02 = row2[0]; //grayImage.readAt(x - 1, y + 1);
                    m12 = row2[1];//grayImage.readAt(x, y + 1);
                    m22 = row2[2];//grayImage.readAt(x + 1, y + 1);



                    mx00 = -1.0f;
                    mx10 = 0.0f;
                    mx20 = 1.0f;

                    mx01 = -2.0f;
                    mx11 = 0.0f;
                    mx21 = 2.0f;

                    mx02 = -1.0f;
                    mx12 = 0.0f;
                    mx22 = 1.0f;


                    my00 = -1.0f;
                    my10 = -2.0f;
                    my20 = -1.0f;

                    my01 = 0.0f;
                    my11 = 0.0f;
                    my21 = 0.0f;

                    my02 = 1.0f;
                    my12 = 2.0f;
                    my22 = 1.0f;


                    float resultX = m00 * mx00 + m10 * mx10 + m20 * mx20 +
                                    m01 * mx01 + m11 * mx11 + m21 * mx21 +
                                    m02 * mx02 + m12 * mx12 + m22 * mx22;

                    float resultY = m00 * my00 + m10 * my10 + m20 * my20 +
                                    m01 * my01 + m11 * my11 + m21 * my21 +
                                    m02 * my02 + m12 * my12 + m22 * my22;



                    result.writeAt(x, y, System.Math.Abs(resultX) + System.Math.Abs(resultY));
                    

                    // shift temporary array
                    row0[0] = row0[1];
                    row0[1] = row0[2];

                    row1[0] = row1[1];
                    row1[1] = row1[2];

                    row2[0] = row2[1];
                    row2[1] = row2[2];




                    // push in now pixels
                    row0[2] = grayImage.readAt(x + 1, y - 1);
                    row1[2] = grayImage.readAt(x + 1, y);
                    row2[2] = grayImage.readAt(x + 1, y + 1);


                }
            }

            return result;
        }
    }
}
