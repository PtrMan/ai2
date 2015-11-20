using System;
using System.Collections.Generic;

using Misc;

namespace ComputationBackend.cs
{
    class ParticleMotionTracker
    {
        // for testing public
        public class TrackedPixel : Datastructures.Grid.Element
        {
            
            public TrackedPixel(int imageSizeX)
            {
                this.imageSizeX = imageSizeX;
            }

            public Vector2<int> position = new Vector2<int>();
            public Vector2<int> oldPosition = new Vector2<int>();
            
            public int age = 0;

            public List<int> neightborIndices = new List<int>();

            public Vector2<float> velocity = new Vector2<float>();

            public Vector2<float> getNormalizedPosition(int imageSizeX)
            {
                Vector2<float> normalizedPosition;

                normalizedPosition = new Vector2<float>();
                normalizedPosition.x = (float)position.x / (float)imageSizeX;
                normalizedPosition.y = (float)position.y / (float)imageSizeX;

                return normalizedPosition;
            }

            // for Datastructures.Grid.Element
            public Vector2<float> getPosition()
            {
                Vector2<float> normalizedPosition;

                normalizedPosition = new Vector2<float>();
                normalizedPosition.x = (float)position.x / (float)imageSizeX;
                normalizedPosition.y = (float)position.y / (float)imageSizeX;

                return normalizedPosition;
            }

            private int imageSizeX;
        }

        public void initialize(OpenCl.ComputeContext computeContext, Vector2<int> imageSize)
        {
            int newTrackingPointCounter;
            int numberOfTrackingPoints;

            const float trackingDensity = 0.2f;
            const int searchRadius = 10;

            this.imageSize = imageSize;

            numberOfTrackingPoints = (int)((float)imageSize.x * (float)imageSize.y * trackingDensity * trackingDensity);

            samplePositions = new Vector2<int>[numberOfTrackingPoints];

            for( newTrackingPointCounter = 0; newTrackingPointCounter < numberOfTrackingPoints; newTrackingPointCounter++ )
            {
                Vector2<int> samplePosition;

                samplePosition = new Vector2<int>();
                samplePosition.x = (int)(((float)imageSize.x - 1.0f) * Misc.RandomUtil.radicalInverse(newTrackingPointCounter, 2));
                samplePosition.y = (int)(((float)imageSize.y - 1.0f) * Misc.RandomUtil.radicalInverse(newTrackingPointCounter, 3));

                samplePositions[newTrackingPointCounter] = samplePosition;
            }

            operatorFindNearestPosition = new OpenCl.OperatorFindNearestPosition();
            operatorFindNearestPosition.initialize(computeContext, searchRadius, imageSize);

            operatorFindNearestPosition.inputPositions = samplePositions;
        }
        
        /*
         * reseeding the tracking points for the edges
         *
         * the chosen samplepoints search for a point *which must be next to the samplepoint*
         * if no point as found the sample is disgarded
         * (because it wouldn't wander around etc.)
         *
         * the functionality needs to be hardwired (not modifyable by genetic algorithm etc)
         */
        public void reseedTrackingPoints(ResourceMetric metric, OpenCl.ComputeContext computeContext, Map2d<bool> edgesImage)
        {
            int newTrackingPointCounter;

            System.Diagnostics.Debug.Assert(edgesImage.getWidth() == imageSize.x);
            System.Diagnostics.Debug.Assert(edgesImage.getLength() == imageSize.y);

            metric.startTimer("visual", "edge point reseed", "findNearestPosition");

            operatorFindNearestPosition.inputMap = edgesImage;
            operatorFindNearestPosition.calculate(computeContext);

            metric.stopTimer();

            metric.startTimer("visual", "edge point reseed", "");

            for( newTrackingPointCounter = 0; newTrackingPointCounter < samplePositions.Length; newTrackingPointCounter++ )
            {
                if( operatorFindNearestPosition.foundNewPositions[newTrackingPointCounter] )
                {
                    Vector2<int> foundPosition;

                    TrackedPixel newTrackedPixel;

                    foundPosition = operatorFindNearestPosition.outputPositions[newTrackingPointCounter];

                    newTrackedPixel = new TrackedPixel(imageSize.x);
                    newTrackedPixel.position = foundPosition.clone();//resultPosition.clone();
                    newTrackedPixel.oldPosition = foundPosition.clone();//resultPosition.clone();

                    trackedBorderPixels.Add(newTrackedPixel);
                }
            }

            metric.stopTimer();

            /*
            for( newTrackingPointCounter = 0; newTrackingPointCounter < samplePositions.Length; newTrackingPointCounter++ )
            {
                Vector2<int> samplePosition;
                Vector2<int> resultPosition;
                bool resultFound;

                const uint searchRadius = 10;

                samplePosition = samplePositions[newTrackingPointCounter];


                resultPosition = Algorithms.Visual.Find.findNearestPositionWhereMapIs(true, samplePosition, edgesImage, searchRadius, out resultFound);
                if( !resultFound )
                {
                    continue;
                }

                // we are here if it found a result point

                TrackedPixel newTrackedPixel;

                newTrackedPixel = new TrackedPixel(imageSize.x);
                newTrackedPixel.position = resultPosition.clone();
                newTrackedPixel.oldPosition = resultPosition.clone();

                trackedBorderPixels.Add(newTrackedPixel);

            }

            metric.stopTimer();
             */
        }

        // DEV< for development public >
        public List<TrackedPixel> trackedBorderPixels = new List<TrackedPixel>();

        private Vector2<int>[] samplePositions; /** \brief are the positions of the points to reseed */
        private Vector2<int> imageSize;

        private ComputationBackend.OpenCl.OperatorFindNearestPosition operatorFindNearestPosition;
    }
}
