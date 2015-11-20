__kernel void findNearestPoint(
	__global int *inputMap,
	__global const int *relativePositions,
	__global int2 *inputPositions,
	__global int2 *outputPositions,
	__global int *foundNewPositions,
	int numberOfRelativePositions,
	int imageWidth,
	int imageHeight
)
{
	const int indexA = get_global_id(0);
	const int indexB = get_global_id(1);

	int index = indexA + indexB * 32;

	int2 inputPosition = inputPositions[index];

	int relativePositionI;

	int foundNewPosition = 0;

	for( relativePositionI = 0; relativePositionI < numberOfRelativePositions; relativePositionI++ )
	{
		int absolutePositionX = relativePositions[relativePositionI*2 + 0] + inputPosition.x;
		int absolutePositionY = relativePositions[relativePositionI*2 + 1] + inputPosition.y;

		if(
			absolutePositionX < 0 || absolutePositionX >= imageWidth ||
			absolutePositionY < 0 || absolutePositionY >= imageHeight
		)
		{
			continue;
		}

		if( inputMap[absolutePositionX + absolutePositionY*imageWidth] != 0 )
		{
			outputPositions[index].x = absolutePositionX;
			outputPositions[index].y = absolutePositionY;

			foundNewPosition = 1;
			break;
		}
	}

	// for debugging
	if( foundNewPosition == 0 )
	{
		outputPositions[index].x = inputPosition.x;
		outputPositions[index].y = inputPosition.y;
	}
	
	foundNewPositions[index] = foundNewPosition;
}
