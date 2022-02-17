using System;
using MapsExt;
using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace GMCObjects
{
    public class MapObjects
    {
        #region Green

        public class CardSpawnPointObject : MapObject
        {
            public Vector3 position;
        }

        [MapObjectSpec(typeof(CardSpawnPointObject))]
        public static class CardSpawnPointSpec
        {
            [MapObjectPrefab] public static GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Spawn Point");

            [MapObjectSerializer]
            public static void Serialize(GameObject instance, CardSpawnPointObject target)
            {
                target.position = instance.transform.position;
            }

            [MapObjectDeserializer]
            public static void Deserialize(CardSpawnPointObject data, GameObject target)
            {
                target.transform.position = data.position;
                GameObject.Destroy(target.GetComponent<SpawnPoint>());
                target.AddComponent<CardSpawnPoint>();
            }
        }

        #endregion
    }
}