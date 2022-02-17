using GMCObjects;
using MapsExt.Editor;
using MapsExt.Editor.MapObjects;
using MapsExt.MapObjects;
using UnityEngine;

namespace GMCObjectsEditor
{

    #region Green
    
    [EditorMapObjectSpec(typeof(MapObjects.CardSpawnPoint), "Green", "Colored blocks | Static")]
    public static class EditorGreen
    {
        [EditorMapObjectPrefab] public static GameObject Prefab => MapObjects.CardSpawnPointSpec.Prefab;

        [EditorMapObjectSerializer]
        public static SerializerAction<MapObjects.CardSpawnPoint> Serialize => EditorSpatialSerializer.BuildSerializer<MapObjects.CardSpawnPoint>(MapObjects.CardSpawnPointSpec.Serialize);

        [EditorMapObjectDeserializer]
        public static DeserializerAction<MapObjects.CardSpawnPoint> Deserialize => EditorSpatialSerializer.BuildDeserializer<MapObjects.CardSpawnPoint>(MapObjects.CardSpawnPointSpec.Deserialize);
    }
    
    #endregion
}