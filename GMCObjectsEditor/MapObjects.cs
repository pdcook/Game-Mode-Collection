using GMCObjects;
using MapsExt.Editor;
using MapsExt.Editor.MapObjects;
using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace GMCObjectsEditor
{
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
}