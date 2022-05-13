using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;
using GameModeCollection.GMCObjects;
using MapsExt.Editor;
using MapsExt;
using GMCObjectsEditor.Visualizers;
using GMCObjectsEditor.EditorActionHandlers;

namespace GMCObjectsEditor 
{
    [EditorMapObjectSpec(typeof(MapObjects.TeleporterObj), "Teleporter", "TRT | Static")]
    public static class EditorTeleporterSpec
    {
        [EditorMapObjectPrefab]
        public static GameObject Prefab => MapObjects.TeleporterSpec.Prefab;

        [EditorMapObjectSerializer]
        public static void Serialize(GameObject instance, MapObjects.TeleporterObj target)
        {
            MapObjects.TeleporterSpec.Serialize(instance, target);
        }

        [EditorMapObjectDeserializer]
        public static void Deserialize(MapObjects.TeleporterObj data, GameObject target)
        {
            MapObjects.TeleporterSpec.Deserialize(data, target);
            target.GetOrAddComponent<TeleporterVisualizer>();
            target.transform.GetChild(0).gameObject.GetOrAddComponent<TeleporterBaseVisualizer>();
            target.transform.GetChild(1).gameObject.GetOrAddComponent<TeleporterBaseVisualizer>();
            target.transform.GetChild(0).gameObject.GetOrAddComponent<SpawnActionHandler>();
            target.transform.GetChild(1).gameObject.GetOrAddComponent<SpawnActionHandler>();
            target.transform.SetAsLastSibling();
        }
    }
}