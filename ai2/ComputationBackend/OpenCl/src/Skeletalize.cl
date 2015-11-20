// orginal algorithm is from
// http://fourier.eng.hmc.edu/e161/lectures/morphology/node3.html

int minArray5(int array[]);

// the algorithm writes non-atomically 1 to changeMade if any change was made
__kernel void narrow(__global int *counterMap, __global int *counterOutputMap, __global int *changeMade, int k, int mapWidth, int mapHeight)
{
	const int indexX = get_global_id(0);
	const int indexY = get_global_id(1);

	//const int x = indexX;
	int y = indexY;
    
	int x;
	for( x = indexX*2; x < (indexX+1)*2; x++ )
	{
		int min;
		int minArray[5];
		int newValue;

		if( x == 0 || x == mapWidth - 1 - 1 )
		{
			continue;
		}

		if( y == 0 || y == mapHeight - 1 - 1 )
		{
			continue;
		}

		if( counterMap[x + y*mapWidth] != k )
		{
			continue;
		}

		changeMade[0] = 1;

		minArray[0] = counterMap[(x) + (y-1)*mapWidth];
        minArray[1] = counterMap[(x-1) + (y)*mapWidth];
        minArray[2] = counterMap[(x) + (y)*mapWidth];
        minArray[3] = counterMap[(x+1) + (y)*mapWidth];
		minArray[4] = counterMap[(x) + (y+1)*mapWidth];

		min = minArray5(minArray);

		newValue = min + 1;
		counterOutputMap[x + y*mapWidth] = newValue;
	}
}

int minArray5(int array[])
{
    int i;
	int result;

	result = array[0];

	for( i = 1; i < 5; i++ )
	{
		if( array[i] < result )
		{
			result = array[i];
		}
	}

	return result;
}
