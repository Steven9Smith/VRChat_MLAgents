using System;
using System.Collections.Generic;
using UdonSharp;
using Unity.MLAgents.Inference.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Unity.MLAgents
{/*
    /// <summary>
    /// Takes a list of floats that encode a sampling distribution and returns the sampling function.
    /// </summary>
    internal static class SamplerFactory
    {
        public static Func<float> CreateUniformSampler(float min, float max, int seed)
        {
            Random distr = new Random(seed);
            return () => min + (float)distr.NextDouble() * (max - min);
        }

        public static Func<float> CreateGaussianSampler(float mean, float stddev, int seed)
        {
            RandomNormal distr = new RandomNormal(seed, mean, stddev);
            return () => (float)distr.NextDouble();
        }

        public static Func<float> CreateMultiRangeUniformSampler(IList<float> intervals, int seed)
        {
            //RNG
            Random distr = new Random(seed);
            // Will be used to normalize intervalFuncs
            float sumIntervalSizes = 0;
            //The number of intervals
            int numIntervals = (intervals.Count / 2);
            // List that will store interval lengths
            float[] intervalSizes = new float[numIntervals];
            // List that will store uniform distributions
            IList<Func<float>> intervalFuncs = new Func<float>[numIntervals];
            // Collect all intervals and store as uniform distrus
            // Collect all interval sizes
            for (int i = 0; i < numIntervals; i++)
            {
                var min = intervals[2 * i];
                var max = intervals[2 * i + 1];
                var intervalSize = max - min;
                sumIntervalSizes += intervalSize;
                intervalSizes[i] = intervalSize;
                intervalFuncs[i] = () => min + (float)distr.NextDouble() * intervalSize;
            }
            // Normalize interval lengths
            for (int i = 0; i < numIntervals; i++)
            {
                intervalSizes[i] = intervalSizes[i] / sumIntervalSizes;
            }
            // Build cmf for intervals
            for (int i = 1; i < numIntervals; i++)
            {
                intervalSizes[i] += intervalSizes[i - 1];
            }
            Multinomial intervalDistr = new Multinomial(seed + 1);
            float MultiRange()
            {
                int sampledInterval = intervalDistr.Sample(intervalSizes);
                return intervalFuncs[sampledInterval].Invoke();
            }

            return MultiRange;
        }
    }*/
    internal static class SamplerFactory
    {
        public static float CreateUniformSampler(float min, float max, int seed)
        {
            UnityEngine.Random.InitState(seed);
            return min + UnityEngine.Random.Range(0f, 1f) * (max - min);
        }

        public static float CreateGaussianSampler(float mean, float stddev, int seed)
        {
            UnityEngine.Random.InitState(seed);
            // Use Box-Muller transform to generate a normally distributed value
            float u1 = 1.0f - UnityEngine.Random.Range(0f, 1f);
            float u2 = 1.0f - UnityEngine.Random.Range(0f, 1f);
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
            return mean + stddev * randStdNormal;
        }

        public static float CreateMultiRangeUniformSampler(IList<float> intervals, int seed)
        {
            UnityEngine.Random.InitState(seed);
            float sumIntervalSizes = 0;
            int numIntervals = intervals.Count / 2;
            float[] intervalSizes = new float[numIntervals];
            float[] cumulativeSizes = new float[numIntervals];

            for (int i = 0; i < numIntervals; i++)
            {
                float min = intervals[2 * i];
                float max = intervals[2 * i + 1];
                float intervalSize = max - min;
                sumIntervalSizes += intervalSize;
                intervalSizes[i] = intervalSize;
            }

            cumulativeSizes[0] = intervalSizes[0] / sumIntervalSizes;
            for (int i = 1; i < numIntervals; i++)
            {
                cumulativeSizes[i] = cumulativeSizes[i - 1] + (intervalSizes[i] / sumIntervalSizes);
            }

            float rand = UnityEngine.Random.Range(0f, 1f);
            int sampledInterval = 0;
            for (int i = 0; i < numIntervals; i++)
            {
                if (rand <= cumulativeSizes[i])
                {
                    sampledInterval = i;
                    break;
                }
            }

            float minInterval = intervals[2 * sampledInterval];
            float maxInterval = intervals[2 * sampledInterval + 1];
            return minInterval + UnityEngine.Random.Range(0f, 1f) * (maxInterval - minInterval);
        }
    }
}
