using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GameModeCollection.Utils
{
    public static class RandomUtils
    {
        public static IEnumerable<T> Shuffled<T>(this IEnumerable<T> sequence)
        {
            return sequence.OrderBy(x => Guid.NewGuid());
        }
        public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector)
        {
            float totalWeight = sequence.Sum(weightSelector);
            // The weight we are after...
            float itemWeightIndex = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeightIndex = 0;

            foreach (var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) })
            {
                currentWeightIndex += item.Weight;

                // If we've hit or passed the weight we are after for this item then it's the one we want....
                if (currentWeightIndex >= itemWeightIndex)
                    return item.Value;

            }
            return default(T);
        }
        /// <summary>
        /// Returns a random element from the sequence, so long as it satisfies the predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T RandomElementWithCondition<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
        {
            return sequence.Where(x => predicate(x)).RandomElementByWeight(x => 1f);
        }            

        public static UnityEngine.Vector2 ClippedGaussianVector2(float minX, float minY, float maxX, float maxY)
        {
            return new UnityEngine.Vector2(ClippedGaussian(minX, maxX), ClippedGaussian(minY, maxY));
        }
        public static float ClippedGaussian(float minValue = 0.0f, float maxValue = 1.0f)
        {
            float u, v, S;

            do
            {
                u = 2.0f * UnityEngine.Random.value - 1.0f;
                v = 2.0f * UnityEngine.Random.value - 1.0f;
                S = u * u + v * v;
            }
            while (S >= 1.0f);

            // Standard Normal Distribution
            float std = u * UnityEngine.Mathf.Sqrt(-2.0f * UnityEngine.Mathf.Log(S) / S);

            // Normal Distribution centered between the min and max value
            // and clamped following the "three-sigma rule"
            float mean = (minValue + maxValue) / 2.0f;
            float sigma = (maxValue - mean) / 3.0f;
            return UnityEngine.Mathf.Clamp(std * sigma + mean, minValue, maxValue);
        }
    }
}
