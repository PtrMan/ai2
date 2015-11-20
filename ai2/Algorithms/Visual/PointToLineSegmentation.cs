using System;
using System.Collections.Generic;

using System.Diagnostics;

using Misc;

// TODO< move into other folder?
namespace Algorithms.Visual
{
    /**
     * class which encapuslates the algorithm which clusters the points based on velocity, distance, position in image plane to lines
     * 
     * could be introspected/replaced by the AI
     * (with visual test)
     */
    class PointToLineSegmentation
    {
        private class Cluster
        {
            public List<ComputationBackend.cs.ParticleMotionTracker.TrackedPixel> trackedPixels = new List<ComputationBackend.cs.ParticleMotionTracker.TrackedPixel>();
        }

        public class ClusteredTrackedPixels
        {
            public List<ComputationBackend.cs.ParticleMotionTracker.TrackedPixel> trackedPixels = new List<ComputationBackend.cs.ParticleMotionTracker.TrackedPixel>();

            public Vector2<float> boundaryMin;
            public Vector2<float> boundaryMax;
            
            public bool isEmpty()
            {
                return trackedPixels.Count == 0;
            }

            public void putNewTrackedPixel(ComputationBackend.cs.ParticleMotionTracker.TrackedPixel newTrackedPixel, int absoluteWidth, int absoluteHeight)
            {
                Vector2<float> relativePosition;

                relativePosition = new Vector2<float>();
                relativePosition.x = newTrackedPixel.position.x / (float)absoluteWidth;
                relativePosition.y = newTrackedPixel.position.y / (float)absoluteHeight;



                if( isEmpty() )
                {
                    boundaryMin = new Vector2<float>();
                    boundaryMax = new Vector2<float>();

                    boundaryMin.x = relativePosition.x;
                    boundaryMin.y = relativePosition.y;
                    boundaryMax.x = relativePosition.x;
                    boundaryMax.y = relativePosition.y;

                    trackedPixels.Add(newTrackedPixel);
                }
                else
                {
                    boundaryMin = Vector2<float>.min(boundaryMin, relativePosition, relativePosition, relativePosition);
                    boundaryMax = Vector2<float>.max(boundaryMax, relativePosition, relativePosition, relativePosition);

                    trackedPixels.Add(newTrackedPixel);
                }
            }

            public bool existsAnyPointWhichIsInEuclideanDistanceTo(Vector2<float> point, float distance, int absoluteWidth, int absoluteHeight)
            {
                if(
                    boundaryMax.x + distance < point.x ||
                    boundaryMin.x - distance > point.x ||
                    boundaryMax.y + distance < point.y ||
                    boundaryMin.y - distance > point.y
                )
                {
                    return false;
                }

                // we are here if there could be a point which is in the right distance

                foreach (ComputationBackend.cs.ParticleMotionTracker.TrackedPixel iterationTrackedPixel in trackedPixels)
                {
                    // is a workaround until the point positions are relative
                    Vector2<float> relativePositionOfTrackedPixel;

                    relativePositionOfTrackedPixel = new Vector2<float>();
                    relativePositionOfTrackedPixel.x = (float)iterationTrackedPixel.position.x / absoluteWidth;
                    relativePositionOfTrackedPixel.y = (float)iterationTrackedPixel.position.y / absoluteHeight;

                    if ((relativePositionOfTrackedPixel - point).magnitude() < distance)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void initialize()
        {
            int i;

            motionClusters = new Cluster[clusterWidth*clusterWidth];

            for( i = 0; i < clusterWidth*clusterWidth; i++ )
            {
                motionClusters[i] = new Cluster();
            }
        }

        // \param maxAbsoluteDistance is the maximal (euclidean) distance of points inside a cluster
        public List<ClusteredTrackedPixels> cluster(List<ComputationBackend.cs.ParticleMotionTracker.TrackedPixel> trackedPixels, float maxDistance, int absoluteWidth, int absoluteHeight)
        {
            resetInternalData();

            putTrackedPixelsIntoClusters(trackedPixels);
            return clusterTrackedPixelsByDistance(maxDistance, absoluteWidth, absoluteHeight);
        }

        private void resetInternalData()
        {
            int clusterI;

            for( clusterI = 0; clusterI < motionClusters.Length; clusterI++ )
            {
                motionClusters[clusterI].trackedPixels.Clear();
            }
        }

        // puts the tracked pixel into the right cluster based on velocity
        private void putTrackedPixelsIntoClusters(List<ComputationBackend.cs.ParticleMotionTracker.TrackedPixel> trackedPixels)
        {
            foreach (ComputationBackend.cs.ParticleMotionTracker.TrackedPixel iterationTrackedPixel in trackedPixels)
            {
                Vector2<float> velocity;
                int clusterIndex;

                velocity = iterationTrackedPixel.velocity;

                clusterIndex = getClusterIndexForVelocity(velocity);
                motionClusters[clusterIndex].trackedPixels.Add(iterationTrackedPixel);
            }
        }

        private int getClusterIndexForVelocity(Vector2<float> velocity)
        {
            int xIndex, yIndex;

            
            if( System.Math.Sqrt(velocity.x*velocity.x + velocity.y*velocity.y) > maxVelocity )
            {
                xIndex = clusterWidth/2 + (int)(velocity.x / maxVelocity);
                yIndex = clusterWidth/2 + (int)(velocity.y / maxVelocity);

                if( xIndex < 0 )
                {
                    xIndex = 0;
                }
                else if( xIndex > clusterWidth/2 )
                {
                    xIndex = clusterWidth - 1;
                }

                if (yIndex < 0)
                {
                    yIndex = 0;
                }
                else if (yIndex > clusterWidth / 2)
                {
                    yIndex = clusterWidth - 1;
                }

            }
            else
            {
                xIndex = clusterWidth/2 + (int)( velocity.x / maxVelocity );
                yIndex = clusterWidth/2 + (int)( velocity.y / maxVelocity );
            }

            System.Diagnostics.Debug.Assert(xIndex >= 0 && xIndex < clusterWidth);
            System.Diagnostics.Debug.Assert(yIndex >= 0 && yIndex < clusterWidth);

            return xIndex + yIndex * clusterWidth;
        }

        // takes the clusters from motionClusters and it clusters the points again to groups by (absolute) distance
        private List<ClusteredTrackedPixels> clusterTrackedPixelsByDistance(float maxDistance, int absoluteWidth, int absoluteHeight)
        {
            List<ClusteredTrackedPixels> result;

            result = new List<ClusteredTrackedPixels>();

            foreach( Cluster iterationCluster in motionClusters )
            {
                List<ClusteredTrackedPixels> allreadyFormedClusters;

                allreadyFormedClusters = new List<ClusteredTrackedPixels>();

                putTrackedPixelsFromClusterIntoFinalClusterByPosition(iterationCluster, allreadyFormedClusters, maxDistance, absoluteWidth, absoluteHeight);

                result.AddRange(allreadyFormedClusters);
            }

            return result;
        }

        private void putTrackedPixelsFromClusterIntoFinalClusterByPosition(Cluster cluster, List<ClusteredTrackedPixels> clusteredTrackedPixels, float clusteringDistance, int absoluteWidth, int absoluteHeight)
        {
            foreach (ComputationBackend.cs.ParticleMotionTracker.TrackedPixel iterationPixel in cluster.trackedPixels)
            {
                bool pointGotClustered;

                pointGotClustered = false;

                Vector2<float> relativeiterationPixelPosition;
                relativeiterationPixelPosition = new Vector2<float>();
                relativeiterationPixelPosition.x = (float)iterationPixel.position.x / absoluteWidth;
                relativeiterationPixelPosition.y = (float)iterationPixel.position.y / absoluteHeight;

                foreach( ClusteredTrackedPixels iterationCluster in clusteredTrackedPixels )
                {
                    if( iterationCluster.existsAnyPointWhichIsInEuclideanDistanceTo(relativeiterationPixelPosition, clusteringDistance, absoluteWidth, absoluteHeight) )
                    {
                        iterationCluster.putNewTrackedPixel(iterationPixel, absoluteWidth, absoluteHeight);

                        pointGotClustered = true;
                    }
                }

                if( !pointGotClustered )
                {
                    // we need to create a new cluster for this point

                    ClusteredTrackedPixels newCluster;

                    newCluster = new ClusteredTrackedPixels();
                    newCluster.putNewTrackedPixel(iterationPixel, absoluteWidth, absoluteHeight);

                    clusteredTrackedPixels.Add(newCluster);
                }
            }

        }


        public float maxVelocity;
        public int clusterWidth;

        // motion based clusters of tracked points
        // first we cluster the point to the motion "type" because it reduces the number of potentially neightbors hopefully
        private Cluster[] motionClusters;

    }
}
