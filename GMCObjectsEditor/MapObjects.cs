using GameModeCollection.GMCObjects;
using MapsExt.Editor;
using MapsExt.Editor.MapObjects;
using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;
using GMCObjectsEditor.Visualizers;

namespace GMCObjectsEditor
{
    #region CardSpawnPoint
    [EditorMapObjectSpec(typeof(MapObjects.CardSpawnPointObject), "Card SpawnPoint")]
    public static class EditorCardSpawnPointSpec
    {
        [EditorMapObjectPrefab] 
        public static GameObject Prefab => MapObjects.CardSpawnPointSpec.Prefab;
        
        [EditorMapObjectSerializer]
        public static void Serialize(GameObject instance, MapObjects.CardSpawnPointObject target)
        {
            MapObjects.CardSpawnPointSpec.Serialize(instance, target);
        }

        [EditorMapObjectDeserializer]
        public static void Deserialize(MapObjects.CardSpawnPointObject data, GameObject target)
        {
            MapObjects.CardSpawnPointSpec.Deserialize(data, target);
            target.gameObject.GetOrAddComponent<CardSpawnPointVisualizer>();
            target.gameObject.GetOrAddComponent<SpawnActionHandler>();
            target.transform.SetAsLastSibling();
        }
    }
    #endregion
    #region TRT
    #region TRT_Traitor_Door
    [EditorMapObjectSpec(typeof(MapObjects.TraitorDoorObj), "Traitor Door", "TRT")]
    public static class EditorTraitorDoorSpec
    {
        [EditorMapObjectPrefab]
        public static GameObject Prefab => MapObjects.TraitorDoorSpec.Prefab;
        [EditorMapObjectSerializer]
        public static SerializerAction<MapObjects.TraitorDoorObj> Serialize => EditorSpatialSerializer.BuildSerializer<MapObjects.TraitorDoorObj>(MapObjects.TraitorDoorSpec.Serialize);
        [EditorMapObjectDeserializer]
        public static DeserializerAction<MapObjects.TraitorDoorObj> Deserialize
        {
            get
            {
                return EditorSpatialSerializer.BuildDeserializer<MapObjects.TraitorDoorObj>(MapObjects.TraitorDoorSpec.Deserialize) + ((data, target) =>
                {
                    target.gameObject.GetOrAddComponent<DetectMapEditor>().IsMapEditor = true;
                });
            }                
        }
    }
    #endregion
    #region TRT_Traitor_Trap
    [EditorMapObjectSpec(typeof(MapObjects.TraitorTrap_GroundDestructibleObj), "Traitor Trap Ground", "TRT")]
    public static class EditorTraitorTrap_GroundDestructibleSpec
    {
        [EditorMapObjectPrefab]
        public static GameObject Prefab => MapObjects.TraitorTrap_GroundDestructibleSpec.Prefab;
        [EditorMapObjectSerializer]
        public static SerializerAction<MapObjects.TraitorTrap_GroundDestructibleObj> Serialize => EditorSpatialSerializer.BuildSerializer<MapObjects.TraitorTrap_GroundDestructibleObj>(MapObjects.TraitorTrap_GroundDestructibleSpec.Serialize);
        [EditorMapObjectDeserializer]
        public static DeserializerAction<MapObjects.TraitorTrap_GroundDestructibleObj> Deserialize
        {
            get
            {
                return EditorSpatialSerializer.BuildDeserializer<MapObjects.TraitorTrap_GroundDestructibleObj>(MapObjects.TraitorTrap_GroundDestructibleSpec.Deserialize) + ((data, target) =>
                {
                    target.gameObject.GetOrAddComponent<DetectMapEditor>().IsMapEditor = true;
                });
            }                
        }
    }
    [EditorMapObjectSpec(typeof(MapObjects.TraitorTrap_BoxDestructibleObj), "Traitor Trap Box", "TRT")]
    public static class EditorTraitorTrap_BoxDestructibleSpec
    {
        [EditorMapObjectPrefab]
        public static GameObject Prefab => MapObjects.TraitorTrap_BoxDestructibleSpec.Prefab;
        [EditorMapObjectSerializer]
        public static SerializerAction<MapObjects.TraitorTrap_BoxDestructibleObj> Serialize => EditorSpatialSerializer.BuildSerializer<MapObjects.TraitorTrap_BoxDestructibleObj>(MapObjects.TraitorTrap_BoxDestructibleSpec.Serialize);
        [EditorMapObjectDeserializer]
        public static DeserializerAction<MapObjects.TraitorTrap_BoxDestructibleObj> Deserialize
        {
            get
            {
                return EditorSpatialSerializer.BuildDeserializer<MapObjects.TraitorTrap_BoxDestructibleObj>(MapObjects.TraitorTrap_BoxDestructibleSpec.Deserialize) + ((data, target) =>
                {
                    target.gameObject.GetOrAddComponent<DetectMapEditor>().IsMapEditor = true;
                });
            }                
        }
    }
    #endregion
    #endregion
}