using System;
using System.Collections.Generic;

namespace GeneticAlgorithm.VisualLowlevel
{
    // genetic algorithm which tries to find the best set of "base vectors" for visual features
    // inspired by paper
    // paper "Sparse Coding with an Overcomplete Basis Set: A Strategy Employed by V1?"
    class VisualLowLevel
    {
        public class PatchSample
        {
            public float[] values;
        }

        private class Rating : GeneticAlgorithm.Core.GeneticAlgorithm.IRating
        {
            public VisualLowLevel visualLowLevel;

            
            public void rating(List<GeneticAlgorithm.Core.Genome> genomes)
            {
                foreach( GeneticAlgorithm.Core.Genome iterationGenome in genomes )
                {
                    rateGenome(iterationGenome);
                }
            }

            private void rateGenome(GeneticAlgorithm.Core.Genome genome)
            {
                int currentGenomeIndex;

                float[] patternTemplate;


                // matching values for all templates
                float[] matchingForTemplates;

                // smaller is better
                float ratingForGeneticAlgorithm;

                ratingForGeneticAlgorithm = 0.0f;

                currentGenomeIndex = 0;

                patternTemplate = getTemplate(ref genome.genome, visualLowLevel.patchWidth, currentGenomeIndex);

                matchingForTemplates = new float[1];

                foreach (PatchSample iterationSample in visualLowLevel.patchSamples)
                {
                    int templateI;

                    float greatestMatchValue;

                    float[] remainingPattern = (float[])iterationSample.values.Clone();

                    matchingForTemplates[0] = calculateMatchingMultiplication(iterationSample.values, patternTemplate);


                    // search for biggest match
                    // TODO

                    greatestMatchValue = matchingForTemplates[0];


                    remainingPattern = multiplyAndSubtractPattern(patternTemplate, greatestMatchValue, remainingPattern);

                    // TODO< loop and do this for the n best matching templates >

                    // now we have in remaining pattern the remaining pattern we try to minimize
                    // (values toward zero are better)
                    // so we translate it into a rating which the genetic algorithm can understand
                    // (higher is better)
                    ratingForGeneticAlgorithm += calculateRatingForGa(remainingPattern);

                    if (float.IsNaN(ratingForGeneticAlgorithm))
                    {
                        calculateRatingForGa(remainingPattern);
                    }
                }

                genome.rating = ratingForGeneticAlgorithm;
            }

            // claculates the image of the template (gaussian with cauchy as described in the paper)
            static private float[] calculateTemplate(int width, float cauchyStrength, float gaussianCenterX, float gaussianCenterY, float gaussianXAxisScale, float gaussianYAxisScale, float gaussianAngle)
            {
                float[] resultPattern;
                int xAsInt;
                int yAsInt;
                float x;
                float y;

                Math.Matrix44 gaussianTransformation;
                Math.Matrix44 gaussianTransformationRotation;
                Math.Matrix44 gaussianTransformationTranslation;
                Math.Matrix44 gaussianTransformationScale;

                resultPattern = new float[width * width];

                gaussianTransformationRotation = Math.Matrix44.createRotationZ((float)System.Math.Cos(gaussianAngle), (float)System.Math.Sin(gaussianAngle));
                gaussianTransformationTranslation = Math.Matrix44.createTranslation(gaussianCenterX, gaussianCenterY, 0.0f);
                gaussianTransformationScale = Math.Matrix44.createScale(gaussianXAxisScale, gaussianYAxisScale, 1.0f);

                gaussianTransformation = gaussianTransformationScale;
                gaussianTransformation.mul(gaussianTransformationRotation);
                gaussianTransformation.mul(gaussianTransformationTranslation);

                for( yAsInt = 0; yAsInt < width; yAsInt++ )
                {
                    for( xAsInt = 0; xAsInt < width; xAsInt++ )
                    {
                        float gaussianResult;
                        float cauchyResult;

                        Math.Vector3<float> gaussianSamplePosition;

                        x = (float)xAsInt / (float)width;
                        y = (float)yAsInt / (float)width;

                        gaussianSamplePosition = gaussianTransformation.mulVector3f(new Math.Vector3<float>(x, y, 0.0f));

                        gaussianResult = calculateGaussian(gaussianSamplePosition.x, gaussianSamplePosition.y);
                        cauchyResult = calculateCauchy(gaussianSamplePosition.x, gaussianSamplePosition.y) * cauchyStrength;

                        resultPattern[xAsInt + yAsInt*width] = gaussianResult * cauchyResult;
                    }
                }

                return resultPattern;
            }

            static private float calculateCauchy(float x, float y)
            {
                float distance;

                const float cauchyS = 1.0f;

                distance = (float)System.Math.Sqrt(x * x + y * y);

                return CauchyDistributionClass.cauchyDistribution(distance, cauchyS, 0.0f);
            }

            static private float calculateGaussian(float x, float y)
            {
                float distance;

                const float gaussianDelta = 0.1f;

                distance = (float)System.Math.Sqrt(x * x + y * y);

                return Misc.Gaussian.calculateGaussianDistribution(distance, 0.0f, gaussianDelta);
            }

            static private float[] multiplyAndSubtractPattern(float[] pattern, float scale, float[] subtractor)
            {
                int i;
                float[] result;

                result = new float[pattern.Length];

                for( i = 0; i < pattern.Length; i++ )
                {
                    result[i] = pattern[i] * scale - subtractor[i];
                }

                return result;
            }

            static private float calculateMatchingMultiplication(float[] pattern, float[] templatePattern)
            {
                float rating;
                int i;

                rating = 0.0f;

                for (i = 0; i < pattern.Length; i++)
                {
                    rating += (pattern[i] * templatePattern[i]);
                }

                return rating;
            }


            static private float calculateRatingForGa(float[] pattern)
            {
                float rating;
                int i;

                rating = 0.0f;

                for( i = 0; i < pattern.Length; i++ )
                {
                    float d;

                    d = pattern[i];
                    rating += (d * d);
                }

                return 1.0f - rating / (pattern.Length*pattern.Length);
            }


            public static float[] getTemplate(ref bool[] genome, int patchWidth, int currentGenomeIndex)
            {
                float cauchyStrength;
                float gaussianCenterX;
                float gaussianCenterY;

                float gaussianXAxisScale;
                float gaussianYAxisScale;
                float gaussianAngle;

                float[] patternTemplate;

                interpretGenom(ref genome, currentGenomeIndex, out cauchyStrength, out gaussianCenterX, out gaussianCenterY, out gaussianXAxisScale, out gaussianYAxisScale, out gaussianAngle);
                patternTemplate = calculateTemplate(patchWidth, cauchyStrength, gaussianCenterX, gaussianCenterY, gaussianXAxisScale, gaussianYAxisScale, gaussianAngle);

                return patternTemplate;
            }


            static private void interpretGenom(ref bool[] genome, int currentGenomeIndex, out float cauchyStrength, out float gaussianCenterX, out float gaussianCenterY, out float gaussianXAxisScale, out float gaussianYAxisScale, out float gaussianAngle)
            {
                int cauchyStrengthAsUint;
                int gaussianCenterXAsUint;
                int gaussianCenterYAsUint;
                int gaussianXAxisScaleAsUint;
                int gaussianYAxisScaleAsUint;
                int gaussianAngleAsUint;

                cauchyStrengthAsUint = GeneticAlgorithm.Core.Helper.toUint(genome, currentGenomeIndex, 16);
                cauchyStrength = (float)cauchyStrengthAsUint / (float)short.MaxValue;

                gaussianCenterXAsUint = GeneticAlgorithm.Core.Helper.toUint(genome, currentGenomeIndex + 16, 16);
                gaussianCenterX = ((float)gaussianCenterXAsUint / (float)short.MaxValue);

                gaussianCenterYAsUint = GeneticAlgorithm.Core.Helper.toUint(genome, currentGenomeIndex + 32, 16);
                gaussianCenterY = ((float)gaussianCenterYAsUint / (float)short.MaxValue);

                gaussianXAxisScaleAsUint = GeneticAlgorithm.Core.Helper.toUint(genome, currentGenomeIndex + 48, 6);
                gaussianXAxisScale = (float)gaussianXAxisScaleAsUint / 64.0f;

                gaussianYAxisScaleAsUint = GeneticAlgorithm.Core.Helper.toUint(genome, currentGenomeIndex + 48 + 6, 6);
                gaussianYAxisScale = (float)gaussianYAxisScaleAsUint / 64.0f;

                gaussianAngleAsUint = GeneticAlgorithm.Core.Helper.toUint(genome, currentGenomeIndex + 48 + 6 + 6, 6);
                gaussianAngle = ((float)gaussianAngleAsUint / 64.0f) * 2.0f * (float)System.Math.PI;
            }
        }

        public void work(int numberOfIterations)
        {
            int iterationI;

            Rating ratingInstance = new Rating();
            ratingInstance.visualLowLevel = this;


            geneticAlgorithm = new Core.GeneticAlgorithm(ratingInstance);

            geneticAlgorithm.genomeBits = (48 + 6 + 6 + 6);
            geneticAlgorithm.initPool();

            // just for testing an infinite loop

            for (iterationI = 0; iterationI < numberOfIterations; iterationI++ )
            {
                geneticAlgorithm.run();

                System.Console.WriteLine(string.Format("highest rating: {0}", geneticAlgorithm.highestRating));
            }

            // TODO
        }

        /**
         * 
         * \param templates will hold the result with the templates
         * 
         * 
         * 
         *
         */
        public void getBestTemplates(List<float[]> templates)
        {
            GeneticAlgorithm.Core.Genome bestGenome;
            int bestGenomeIndex;

            templates.Clear();
            
            bestGenome = geneticAlgorithm.getBestGenome(out bestGenomeIndex);

            templates.Add( Rating.getTemplate(ref bestGenome.genome, patchWidth, 0) );
        }

        private GeneticAlgorithm.Core.GeneticAlgorithm geneticAlgorithm;

        public int patchWidth;
        public List<PatchSample> patchSamples = new List<PatchSample>();
    }
}
