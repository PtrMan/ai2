__kernel void blurX(__global float *inputMap, __global const float *kernelArray, __global float *outputMap, int radius, int mapWidth)
{
	const int indexX = get_global_id(0);
	const int indexY = get_global_id(1);

	//const int x = indexX;
	int y = indexY;
    
	int x;
	for( x = indexX*2; x < (indexX+1)*2; x++ )
	{
	
		int ir;

		float temp = 0.0f;

		for( ir = -radius-1; ir < radius-1; ir++ )
		{
			if( (ir+x) < 0 || (ir+x) >= mapWidth )
			{
				//continue;
			}
			else
			{
				temp = temp + (inputMap[(x+ir) + (y) * mapWidth] * kernelArray[radius-1+ir]);
			}
		}

		outputMap[x + y*mapWidth] = temp;
	}
}

__kernel void blurY(__global float *inputMap, __global const float *kernelArray, __global float *outputMap, int radius, int mapWidth, int mapHeight)
{
	const int indexX = get_global_id(0);
	const int indexY = get_global_id(1);

	//const int x = indexX;
	int y = indexY;
	
    int x;
	for( x = indexX*2; x < (indexX+1)*2; x++ )
	{
		int ir;

		float temp = 0.0f;

		for( ir = -radius-1; ir < radius-1; ir++ )
		{
			if( ir+y < 0 || ir+y >= mapHeight )
			{
				continue;
			}

			temp = temp + (inputMap[(x) + (y+ir) * mapWidth] * kernelArray[radius-1+ir]);
		}

		outputMap[x + y*mapWidth] = temp;
	}
}
