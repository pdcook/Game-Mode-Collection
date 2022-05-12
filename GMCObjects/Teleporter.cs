using UnityEngine;
using MapsExt;
using MapsExt.MapObjects;
using HarmonyLib;
namespace GameModeCollection.GMCObjects
{
    public class Teleporter : MapObject
    {
        public Vector3 startPosition = Vector3.up;
        public Vector3 endPosition = Vector3.down;

        public override MapObject Move(Vector3 v)
        {
            var copy = (Teleporter)AccessTools.Constructor(this.GetType()).Invoke(new object[] { });
            copy.active = this.active;
            copy.startPosition = this.startPosition + v;
            copy.endPosition = this.endPosition + v;
            return copy;
        }

        public override string ToString()
        {
            return $"Teleporter[{this.startPosition}, {this.endPosition}]";
        }
    }

    [MapObjectSpec(typeof(Teleporter))]
    public static class TeleporterSpec
    {
        [MapObjectPrefab]
        public static GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Rope");

        [MapObjectSerializer]
        public static void Serialize(GameObject instance, Teleporter target)
        {
            target.startPosition = instance.transform.position;
            target.endPosition = instance.transform.GetChild(0).position;
        }

        [MapObjectDeserializer]
        public static void Deserialize(Teleporter data, GameObject target)
        {
            target.transform.position = data.startPosition;
            target.transform.GetChild(0).position = data.endPosition;
        }
    }
}
