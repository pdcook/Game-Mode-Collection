namespace GameModeCollection.Utils
{
    public static class RandomUtils
    {
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
