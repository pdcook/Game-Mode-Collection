using System;
using UnityEngine;

namespace GameModeCollection.GMCObjects
{
    public static class TransformExtension
    {
        public static Transform TryGetChild(this Transform t, int index)
        {
            try
            {
                return t.GetChild(index);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }
    }
}