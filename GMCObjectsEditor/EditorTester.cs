using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;
using GameModeCollection.GMCObjects;
using MapsExt.Editor;
using MapsExt.MapObjects;
using MapsExt;
using GMCObjectsEditor.Visualizers;
using GMCObjectsEditor.EditorActionHandlers;
using MapsExt.Editor.MapObjects;

namespace GMCObjectsEditor 
{
    [EditorMapObjectSpec(typeof(MapObjects.TraitorTesterObj), "Traitor Tester", "TRT")]
    public static class EditorTraitorTesterSpec
    {
        [EditorMapObjectPrefab]
        public static GameObject Prefab => MapObjects.TraitorTesterSpec.Prefab;

        /*
        [EditorMapObjectSerializer]
        public static void Serialize(GameObject instance, MapObjects.TraitorTesterObj target)
        {
            MapObjects.TraitorTesterSpec.Serialize(instance, target);
        }

        [EditorMapObjectDeserializer]
        public static void Deserialize(MapObjects.TraitorTesterObj data, GameObject target)
        {
            MapObjects.TraitorTesterSpec.Deserialize(data, target);
            target.GetOrAddComponent<TesterVisualizer>();
            target.transform.GetChild(0).gameObject.GetOrAddComponent<TesterButtonVisualizer>();
            target.transform.GetChild(1).gameObject.GetOrAddComponent<TesterRoomVisualizer>();
            target.transform.GetChild(0).gameObject.GetOrAddComponent<SpawnActionHandler>();
            target.transform.GetChild(1).gameObject.GetOrAddComponent<SpawnActionHandler>();

            target.transform.SetAsLastSibling();

            target.gameObject.GetOrAddComponent<DetectMapEditor>().IsMapEditor = true;
        }
        */
        [EditorMapObjectSerializer]
        public static SerializerAction<MapObjects.TraitorTesterObj> Serialize => EditorSpatialSerializer.BuildSerializer<MapObjects.TraitorTesterObj>(MapObjects.TraitorTesterSpec.Serialize);
        [EditorMapObjectDeserializer]
        public static DeserializerAction<MapObjects.TraitorTesterObj> Deserialize
        {
            get
            {
                return EditorSpatialSerializer.BuildDeserializer<MapObjects.TraitorTesterObj>(MapObjects.TraitorTesterSpec.Deserialize) + ((data, target) =>
                {
                    target.GetOrAddComponent<TesterVisualizer>();
                    target.transform.GetChild(0).gameObject.GetOrAddComponent<TesterButtonVisualizer>();
                    target.transform.GetChild(1).gameObject.GetOrAddComponent<TesterRoomVisualizer>();
                    target.transform.GetChild(2).gameObject.GetOrAddComponent<TesterLightVisualizer>();
                    target.transform.GetChild(0).gameObject.GetOrAddComponent<SpawnActionHandler>();
                    target.transform.GetChild(1).gameObject.GetOrAddComponent<SpawnActionHandler>();
                    target.transform.GetChild(2).gameObject.GetOrAddComponent<SpawnActionHandler>();

                    target.transform.SetAsLastSibling();
                    target.transform.GetChild(0).gameObject.GetOrAddComponent<DetectMapEditor>().IsMapEditor = true;
                });
            }
        }
    }
}