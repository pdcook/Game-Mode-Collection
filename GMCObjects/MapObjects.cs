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
            public Vector3 OpenerPos;
        }

        [MapObjectSpec(typeof(TraitorDoorObj))]
        public static class TraitorDoorSpec
        {   
            private static GameObject _prefab = null;
            [MapObjectPrefab] public static GameObject Prefab
            {
                get
                {
                    if (_prefab == null)
                    {
                        GameObject door = GameObject.Instantiate(MapObjectManager.LoadCustomAsset<GameObject>("Ground"));

                        GameObject spawnPoint = MapObjectManager.LoadCustomAsset<GameObject>("Spawn Point");
                        GameObject.Instantiate(spawnPoint, door.transform.position - Vector3.right, Quaternion.identity, door.transform).name = "AutoOpener";
                        _prefab = door;
                        _prefab.SetActive(false);
                        GameObject.DontDestroyOnLoad(_prefab);
                    }
                    return _prefab;

                }
            }

            [MapObjectSerializer]
            public static void Serialize(GameObject instance, TraitorDoorObj target)
            {
                target.OpenerPos = instance.transform.GetChild(0).position;
                SpatialSerializer.Serialize(instance, target);
            }

            [MapObjectDeserializer]
            public static void Deserialize(TraitorDoorObj data, GameObject target)
            {
                SpatialSerializer.Deserialize(data, target);

                GameObject.Destroy(target.transform.GetChild(0).GetComponent<SpawnPoint>());

                // undo transforming caused by parenting
                Transform child0 = target.transform.GetChild(0);
                child0.rotation = Quaternion.identity;
                
                child0.SetParent(null);
                child0.localScale = Vector3.one;
                child0.SetParent(target.transform);
                child0.position = data.OpenerPos;
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
        #region LightSwitch
        public class LightSwitchObj : MapObject
        {
            public Vector3 Loc;
        }

        [MapObjectSpec(typeof(LightSwitchObj))]
        public static class LightSwitchSpec
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
                        _prefab.name = "LightSwitch";
                        GameObject.DontDestroyOnLoad(_prefab);
                    }
                    return _prefab;

                }
            }
            [MapObjectSerializer]
            public static void Serialize(GameObject instance, LightSwitchObj target)
            {
                target.Loc = instance.transform.position;
            }

            [MapObjectDeserializer]
            public static void Deserialize(LightSwitchObj data, GameObject target)
            {
                target.transform.position = data.Loc;
                GameObject.Destroy(target.GetComponent<SpawnPoint>());
                target.AddComponent<LightSwitch>();
            }
        }
        #endregion
        #endregion
        #region TRT_Intercom
        public class IntercomObj : MapObject
        {
            public Vector3 Loc;
        }

        [MapObjectSpec(typeof(IntercomObj))]
        public static class IntercomSpec
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
                        _prefab.name = "TRT Intercom";
                        GameObject.DontDestroyOnLoad(_prefab);
                    }
                    return _prefab;

                }
            }
            [MapObjectSerializer]
            public static void Serialize(GameObject instance, IntercomObj target)
            {
                target.Loc = instance.transform.position;
            }

            [MapObjectDeserializer]
            public static void Deserialize(IntercomObj data, GameObject target)
            {
                target.transform.position = data.Loc;
                GameObject.Destroy(target.GetComponent<SpawnPoint>());
                target.AddComponent<TRT_Intercom>();
            }
        }
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
        #region TraitorTester 
        public class TraitorTesterObj : SpatialMapObject
        {
            public Vector3 buttonLoc;
            public Vector3 roomLoc;
            public Vector3 lightLoc;
        }

        [MapObjectSpec(typeof(TraitorTesterObj))]
        public static class TraitorTesterSpec
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
                        //GameObject testerParent = new GameObject("TraitorTester");
                        GameObject testerParent = GameObject.Instantiate(MapObjectManager.LoadCustomAsset<GameObject>("Ground"));
                        testerParent.name = "TraitorTester";
                        GameObject.Instantiate(spawnPoint, testerParent.transform.position + 5f*Vector3.right, Quaternion.identity, testerParent.transform).name = "Button Loc";
                        GameObject.Instantiate(spawnPoint, testerParent.transform.position - 5f*Vector3.right, Quaternion.identity, testerParent.transform).name = "Room Loc";
                        GameObject lightPos = GameObject.Instantiate(spawnPoint, testerParent.transform.position + 5f*Vector3.up, Quaternion.identity, testerParent.transform);
                        lightPos.name = "Light Pos";
                        GameObject lightAsset = GameModeCollection.TRT_Assets.LoadAsset<GameObject>("TRT_TesterLight");
                        if (lightAsset is null)
                        {
                            GameModeCollection.LogError("Could not load TRT_TesterLight");
                        }
                        GameObject light = GameObject.Instantiate(lightAsset, lightPos.transform.position + 5f*Vector3.up, Quaternion.identity, lightPos.transform);
                        light.name = "TRT_TesterLight";
                        light.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                        light.GetComponent<Canvas>().sortingLayerID = SortingLayer.NameToID("MostFront");
                        light.GetComponent<Canvas>().sortingOrder = 100;
                        //light.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("MostFront");
                        //light.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("MostFront");
                        _prefab = testerParent;
                        _prefab.SetActive(false);
                        GameObject.DontDestroyOnLoad(_prefab);
                    }
                    return _prefab;

                }
            }

            [MapObjectSerializer]
            public static void Serialize(GameObject instance, TraitorTesterObj target)
            {
                target.buttonLoc = instance.transform.GetChild(0).position;
                target.roomLoc = instance.transform.GetChild(1).position;
                target.lightLoc = instance.transform.GetChild(2).position;

                // serialize door
                SpatialSerializer.Serialize(instance, target);
            }

            [MapObjectDeserializer]
            public static void Deserialize(TraitorTesterObj data, GameObject target)
            {
                // deserialize the door
                SpatialSerializer.Deserialize(data, target);

                GameObject.Destroy(target.transform.GetChild(0).GetComponent<SpawnPoint>());
                GameObject.Destroy(target.transform.GetChild(1).GetComponent<SpawnPoint>());
                GameObject.Destroy(target.transform.GetChild(2).GetComponent<SpawnPoint>());

                // undo transforming caused by parenting
                Transform child0 = target.transform.GetChild(0);
                Transform child1 = target.transform.GetChild(1);
                Transform child2 = target.transform.GetChild(2);
                child0.rotation = Quaternion.identity;
                child1.rotation = Quaternion.identity;
                child2.rotation = Quaternion.identity;
                
                child0.SetParent(null);
                child1.SetParent(null);
                child2.SetParent(null);
                child0.localScale = Vector3.one;
                child1.localScale = Vector3.one;
                child2.localScale = Vector3.one;
                child0.SetParent(target.transform);
                child1.SetParent(target.transform);
                child2.SetParent(target.transform);
                child0.position = data.buttonLoc;
                child1.position = data.roomLoc;
                child2.position = data.lightLoc;


                target.AddComponent<TraitorTester>();
                target.AddComponent<TesterDoor>();
            }
        }
        #endregion
    }
}