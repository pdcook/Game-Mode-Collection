using System;
using MapsExt;
using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;
using HarmonyLib;

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
                target.AddComponent<TraitorDoor>();
            }
        }
        public class TeleporterObj : MapObject
        {
            public Vector3 loc1;
            public Vector3 loc2;
        }

        [MapObjectSpec(typeof(TeleporterObj))]
        public static class TeleporterSpec
        {
            private static GameObject _prefab = null;
            [MapObjectPrefab] public static GameObject Prefab
            {
                get
                {
                    if (_prefab == null)
                    {
                        GameObject spawnPoint = MapObjectManager.LoadCustomAsset<GameObject>("Spawn Point");
                        GameObject teleporterParent = new GameObject("Teleporter");
                        GameObject.Instantiate(spawnPoint, teleporterParent.transform.position + Vector3.right, Quaternion.identity, teleporterParent.transform).name = "Loc 1";
                        GameObject.Instantiate(spawnPoint, teleporterParent.transform.position - Vector3.right, Quaternion.identity, teleporterParent.transform).name = "Loc 2";
                        _prefab = teleporterParent;
                        GameObject.DontDestroyOnLoad(_prefab);
                    }
                    return _prefab;

                }
            }

            [MapObjectSerializer]
            public static void Serialize(GameObject instance, TeleporterObj target)
            {
                target.loc1 = instance.transform.GetChild(0).position;
                target.loc2 = instance.transform.GetChild(1).position;
            }

            [MapObjectDeserializer]
            public static void Deserialize(TeleporterObj data, GameObject target)
            {
                target.transform.GetChild(0).position = data.loc1;
                target.transform.GetChild(1).position = data.loc2;
                GameObject.Destroy(target.transform.GetChild(0).GetComponent<SpawnPoint>());
                GameObject.Destroy(target.transform.GetChild(1).GetComponent<SpawnPoint>());
                target.AddComponent<Teleporter>();
            }
        }        

        #endregion
    }
}