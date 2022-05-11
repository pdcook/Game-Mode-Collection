using System;
using MapsExt;
using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace GameModeCollection.GMCObjects
{
    public class MapObjects
    {
        #region CardSpawnPoints

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

        #region TRT_Traitor

        public class TraitorDoorObj : SpatialMapObject
        {
            // a door that can be opened by a traitor, it always opens to the local up direction, and closes automatically
        }

        [MapObjectSpec(typeof(TraitorDoorObj))]
        public static class TraitorDoorSpec
        {
            [MapObjectPrefab] public static GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground");

            [MapObjectSerializer]
            public static void Serialize(GameObject instance, TraitorDoorObj target)
            {
                SpatialSerializer.Serialize(instance, target);
            }

            [MapObjectDeserializer]
            public static void Deserialize(TraitorDoorObj data, GameObject target)
            {
                SpatialSerializer.Deserialize(data, target);
                // this would be where we would add the door component and the interaction gameobjects
                target.AddComponent<TraitorDoor>();
            }
        }

        #endregion
    }
}