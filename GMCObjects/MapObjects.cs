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

        #region TRT
        #region TRT_Traitor_Door

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
        #endregion
        #region TRT_Traitor_Trap 
        public class TraitorTrap_GroundDestructibleObj : SpatialMapObject
        {
        }

        [MapObjectSpec(typeof(TraitorTrap_GroundDestructibleObj))]
        public static class TraitorTrap_GroundDestructibleSpec
        {
            [MapObjectPrefab] public static GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground");

            [MapObjectSerializer]
            public static void Serialize(GameObject instance, TraitorTrap_GroundDestructibleObj target)
            {
                SpatialSerializer.Serialize(instance, target);
            }

            [MapObjectDeserializer]
            public static void Deserialize(TraitorTrap_GroundDestructibleObj data, GameObject target)
            {
                SpatialSerializer.Deserialize(data, target);
                target.AddComponent<TraitorTrap_Destructible>();
            }
        }
        public class TraitorTrap_BoxDestructibleObj : SpatialMapObject
        {
        }

        [MapObjectSpec(typeof(TraitorTrap_BoxDestructibleObj))]
        public static class TraitorTrap_BoxDestructibleSpec
        {
            [MapObjectPrefab] public static GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box");

            [MapObjectSerializer]
            public static void Serialize(GameObject instance, TraitorTrap_BoxDestructibleObj target)
            {
                SpatialSerializer.Serialize(instance, target);
            }

            [MapObjectDeserializer]
            public static void Deserialize(TraitorTrap_BoxDestructibleObj data, GameObject target)
            {
                SpatialSerializer.Deserialize(data, target);
                target.AddComponent<TraitorTrap_Destructible>();
            }
        }
        public class JammerObj : MapObject
        {
            public Vector3 Loc;
        }

        [MapObjectSpec(typeof(JammerObj))]
        public static class JammerSpec
        {
            private static GameObject _prefab = null;
            [MapObjectPrefab]
            public static GameObject Prefab
            {
                get
                {
                    if (_prefab == null)
                    {
                        GameObject spawnPoint = MapObjectManager.LoadCustomAsset<GameObject>("Spawn Point");
                        _prefab = GameObject.Instantiate(spawnPoint);
                        _prefab.name = "Jammer";
                        GameObject.DontDestroyOnLoad(_prefab);
                    }
                    return _prefab;

                }
            }
            [MapObjectSerializer]
            public static void Serialize(GameObject instance, JammerObj target)
            {
                target.Loc = instance.transform.position;
            }

            [MapObjectDeserializer]
            public static void Deserialize(JammerObj data, GameObject target)
            {
                target.transform.position = data.Loc;
                GameObject.Destroy(target.GetComponent<SpawnPoint>());
                target.AddComponent<TraitorTrap_Jammer>();
            }
        }
        #endregion
        #endregion
        #region Teleporter 
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