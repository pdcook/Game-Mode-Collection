using UnityEngine;
using System.Linq;
using System.Collections.Generic;
namespace GameModeCollection.Extensions
{
    public static class TransformExtensions
    {
        public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
        {
            transform.localScale = Vector3.one;
            transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
        }
        public static int[] GetSiblingIndexTree(this Transform transform)
        {
            List<int> indeces = new List<int>() { };
            Transform cur = transform;
            while (cur.parent != null)
            {
                indeces.Add(cur.GetSiblingIndex());
                cur = cur.parent;
            }
            return indeces.ToArray();
        }
    }
}
