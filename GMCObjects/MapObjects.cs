using System;
using MapsExt;
using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace GMCObjects
{
    public class MapObjects
    {
        private static readonly Material defaultMaterial = new Material(Shader.Find("Sprites/Default"));

        public static void Deserialize(GameObject target, Color color)
        {
            GMCObjects.instance.ExecuteAfterFrames(1, () =>
            {
                GameObject.Destroy(target.GetComponent<SpriteMask>());
                target.GetComponent<SpriteRenderer>().material = defaultMaterial;
                color.a = 1;
                target.GetComponent<SpriteRenderer>().color = color;
            });
            GMCObjects.instance.ExecuteAfterFrames(5, () =>
            {
                GameObject.Destroy(target.GetComponent<SpriteMask>());
                target.GetComponent<SpriteRenderer>().material = defaultMaterial;
                color.a = 1;
                target.GetComponent<SpriteRenderer>().color = color;
            });
        }

        #region Green

        public class CardSpawnPoint : SpatialMapObject
        {
        }

        [MapObjectSpec(typeof(CardSpawnPoint))]
        public static class CardSpawnPointSpec
        {
            [MapObjectPrefab] public static GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground");

            [MapObjectSerializer]
            public static void Serialize(GameObject instance, CardSpawnPoint target)
            {
                SpatialSerializer.Serialize(instance, target);
            }

            [MapObjectDeserializer]
            public static void Deserialize(CardSpawnPoint data, GameObject target)
            {
                SpatialSerializer.Deserialize(data, target);
                MapObjects.Deserialize(target, Color.green*0.8f);
            }
        }

        #endregion
    }
}