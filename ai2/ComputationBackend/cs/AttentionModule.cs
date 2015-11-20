using System;
using System.Collections.Generic;

using Misc;

namespace ComputationBackend.cs
{
    // attention algorithm
    // http://ilab.usc.edu/surprise/

    // PATENT UNKNOWN
    class AttentionModule
    {
        private class NovelityDetector
        {
            private float alpha = 1.5f; // 1 <= x <= 2 because of used approximation
            private float beta = 0.1f; // acutally a parameter

            private float surprise;

            public void timestep(float forgettingFactor, float lambda)
            {
                float alphaTick, betaTick;
                float gammaAlphaTick, gammaAlpha;

                alphaTick = forgettingFactor * alpha + lambda;
                betaTick = forgettingFactor * beta + 1.0f;

                gammaAlpha = (float)Math.Gamma.lanczosGamma(alpha);
                gammaAlphaTick = (float)Math.Gamma.lanczosGamma(alphaTick);

                surprise = alphaTick * (float)System.Math.Log(beta / betaTick) + (float)System.Math.Log(gammaAlphaTick / gammaAlpha) + betaTick * (alpha / beta) + (alpha - alphaTick) * ((float)Math.Gamma.diGamma(alpha));

                //System.Diagnostics.Debug.Assert(surprise >= 0.0f && surprise < 5000000.0f);

                alpha = alphaTick;
                ///beta = betaTick;
            }

            public float getSurprise()
            {
                return surprise;
            }
        }

        public void initialize(OpenCl.ComputeContext computeContext, Vector2<int> mapSize)
        {
            blurMotionMap.initialize(computeContext, 80, mapSize);

            allocateNovelityDetectors(mapSize);
            allocateDownsampledMaps(mapSize);
            allocateMasterNovelity(mapSize);
        }

        private void allocateMasterNovelity(Vector2<int> mapSize)
        {
            int downsampleDivisionFactor;
            Vector2<int> downsampledMapSize;

            downsampleDivisionFactor = Misc.Math.powi(2, featureMapDownsamplePower);

            downsampledMapSize = new Vector2<int>();
            downsampledMapSize.x = mapSize.x / downsampleDivisionFactor;
            downsampledMapSize.y = mapSize.y / downsampleDivisionFactor;

            masterNovelity = new float[downsampledMapSize.x * downsampledMapSize.y];
        }

        private void allocateNovelityDetectors(Vector2<int> mapSize)
        {
            int downsampleDivisionFactor;
            Vector2<int> downsampledMapSize;
            int i;


            downsampleDivisionFactor = Misc.Math.powi(2, featureMapDownsamplePower);

            downsampledMapSize = new Vector2<int>();
            downsampledMapSize.x = mapSize.x / downsampleDivisionFactor;
            downsampledMapSize.y = mapSize.y / downsampleDivisionFactor;

            featureMapSize = downsampledMapSize.clone();

            novelityMotion = new NovelityDetector[downsampledMapSize.x * downsampledMapSize.y];

            for( i = 0; i < downsampledMapSize.x * downsampledMapSize.y; i++ )
            {
                novelityMotion[i] = new NovelityDetector();
            }
        }

        private void allocateDownsampledMaps(Vector2<int> mapSize)
        {
            int downsamplePowerCounter;
            int downsampleFactor;

            // allocate arrays
            motionDownsampled = new Map2d<float>[featureMapDownsamplePower];


            // fill arrays with maps
            downsampleFactor = 2;

            for( downsamplePowerCounter = 0; downsamplePowerCounter < featureMapDownsamplePower; downsamplePowerCounter++ )
            {
                System.Diagnostics.Debug.Assert((mapSize.x % downsampleFactor) == 0);
                System.Diagnostics.Debug.Assert((mapSize.y % downsampleFactor) == 0);

                motionDownsampled[downsamplePowerCounter] = new Map2d<float>((uint)(mapSize.x / downsampleFactor), (uint)(mapSize.y / downsampleFactor));

                downsampleFactor *= 2;
            }
        }

        public void calculate(ResourceMetric metric, OpenCl.ComputeContext computeContext)
        {
            // blur motion map
            bluredMotionMap = doBlurMotionMap(metric, computeContext);

            // downsample maps
            downsampleMaps(metric);

            // novelity motion
            calculateNovelity();
            calculateMasterNovelity();
        }

        /**
         * returns the position where the highest attention is reached
         * 
         */
        public Vector2<int> getPositionOfMostAttention()
        {
            Vector2<int> highestAttentionPosition;
            int highestAttentionIndex;
            float highestAttentionValue;
            int i;
            int downsampleDivisionFactor;

            highestAttentionIndex = 0;
            highestAttentionValue = masterNovelity[0];

            for( i = 1; i < masterNovelity.Length; i++ )
            {
                if( masterNovelity[i] > highestAttentionValue )
                {
                    highestAttentionValue = masterNovelity[i];
                    highestAttentionIndex = i;
                }
            }

            // calculate absolute position

            downsampleDivisionFactor = Misc.Math.powi(2, featureMapDownsamplePower);

            highestAttentionPosition = new Vector2<int>();
            highestAttentionPosition.x = (highestAttentionIndex % featureMapSize.x) * downsampleDivisionFactor;
            highestAttentionPosition.y = (highestAttentionIndex / featureMapSize.x) * downsampleDivisionFactor;
            
            return highestAttentionPosition;
        }

        private void calculateNovelity()
        {
            int x, y;

            for( y = 0; y < featureMapSize.y; y++ )
            {
                for( x = 0; x < featureMapSize.x; x++ )
                {
                    float novelityInput;

                    novelityInput = motionDownsampled[featureMapDownsamplePower - 1].readAt(x, y);

                    novelityMotion[x + y * featureMapSize.x].timestep(forgettingFactor, novelityInput);
                }
            }
        }

        private void calculateMasterNovelity()
        {
            int i;

            for( i = 0; i < featureMapSize.x*featureMapSize.y; i++ )
            {
                float novelityCombined;

                novelityCombined = 0.0f;
                novelityCombined += (novelityMotion[i].getSurprise() * 1.0f);

                masterNovelity[i] = novelityCombined;
            }
        }

        private Map2d<float> doBlurMotionMap(ResourceMetric metric, OpenCl.ComputeContext computeContext)
        {
            Map2d<float> bluredMotionMap;

            metric.startTimer("visual attention", "blur motion map", "");

            bluredMotionMap = new Map2d<float>(motionMap.getWidth(), motionMap.getLength());

            blurMotionMap.inputMap = motionMap;
            blurMotionMap.outputMap = bluredMotionMap;

            blurMotionMap.calculate(computeContext);

            metric.stopTimer();

            return bluredMotionMap;
        }

        private void downsampleMaps(ResourceMetric metric)
        {
            int downsamplePowerCounter;
            Map2d<float> sourceMap;
            Map2d<float> destinationMap;

            metric.startTimer("visual attention", "downsample maps", "");

            sourceMap = bluredMotionMap;
            destinationMap = motionDownsampled[0];

            downsampleMap(sourceMap, destinationMap);

            for( downsamplePowerCounter = 0; downsamplePowerCounter < featureMapDownsamplePower-1; downsamplePowerCounter++ )
            {
                sourceMap = motionDownsampled[downsamplePowerCounter];
                destinationMap = motionDownsampled[downsamplePowerCounter + 1];

                downsampleMap(sourceMap, destinationMap);
            }

            metric.stopTimer();
        }

        private static void downsampleMap(Map2d<float> source, Map2d<float> destination)
        {
            int destinationX;
            int destinationY;
            
            System.Diagnostics.Debug.Assert(source.getWidth() / 2 == destination.getWidth());
            System.Diagnostics.Debug.Assert(source.getLength() / 2 == destination.getLength());

            for( destinationY = 0; destinationY < destination.getLength(); destinationY++ )
            {
                for( destinationX = 0; destinationX < destination.getWidth(); destinationX++ )
                {
                    float downsampledValue;

                    downsampledValue  = source.readAt(destinationX * 2, destinationY * 2);
                    downsampledValue += source.readAt(destinationX * 2 + 1, destinationY * 2);
                    downsampledValue += source.readAt(destinationX * 2, destinationY * 2 + 1);
                    downsampledValue += source.readAt(destinationX * 2 + 1, destinationY * 2 + 1);
                    downsampledValue *= 0.25f;

                    destination.writeAt(destinationX, destinationY, downsampledValue);
                }
            }
        }

        public Map2d<float> getMasterNovelityAsMap()
        {
            Map2d<float> result;
            int x, y;

            result = new Map2d<float>((uint)featureMapSize.x, (uint)featureMapSize.y);

            for( y = 0; y < featureMapSize.y; y++ )
            {
                for( x = 0; x < featureMapSize.x; x++ )
                {
                    float value;

                    value = masterNovelity[x + y * featureMapSize.x];
                    result.writeAt(x, y, value);
                }
            }

            return result;
        }

        // maps are allready resized
        public Map2d<float> redGreenMap;
        public Map2d<float> blueYellowMap;

        // this is given from the motion detector module, unblured
        public Map2d<float> motionMap;

        Map2d<float> bluredMotionMap;
        public Map2d<float>[] motionDownsampled;

        // should be configured from outside
        public float forgettingFactor = 1.0f;

        // must be set before configure
        public int featureMapDownsamplePower;

        private Vector2<int> featureMapSize;

        private NovelityDetector[] novelityMotion;

        private float[] masterNovelity;

        private ComputationBackend.OpenCl.OperatorBlur blurMotionMap = new ComputationBackend.OpenCl.OperatorBlur();
    }
}
