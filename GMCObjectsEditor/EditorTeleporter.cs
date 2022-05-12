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
	[EditorMapObjectSpec(typeof(Teleporter), "Teleporter")]
	public static class EditorRopeSpec
	{
		[EditorMapObjectPrefab]
		public static GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Editor Rope");

		[EditorMapObjectSerializer]
		public static void Serialize(GameObject instance, Teleporter target)
		{
			var ropeInstance = instance.GetComponent<EditorTeleporterInstance>();
			target.startPosition = ropeInstance.GetAnchor(0).GetPosition();
			target.endPosition = ropeInstance.GetAnchor(1).GetPosition();
		}

		[EditorMapObjectDeserializer]
		public static void Deserialize(Teleporter data, GameObject target)
		{
			target.transform.GetChild(0).gameObject.GetOrAddComponent<MapObjectAnchor>();
			target.transform.GetChild(0).gameObject.GetOrAddComponent<TeleporterActionHandler>();

			target.transform.GetChild(1).gameObject.GetOrAddComponent<MapObjectAnchor>();
			target.transform.GetChild(1).gameObject.GetOrAddComponent<TeleporterActionHandler>();

			var startCollider = target.transform.GetChild(0).gameObject.GetOrAddComponent<BoxCollider2D>();
			var endCollider = target.transform.GetChild(1).gameObject.GetOrAddComponent<BoxCollider2D>();
			startCollider.size = Vector2.one * 1;
			endCollider.size = Vector2.one * 1;

			var instance = target.GetOrAddComponent<EditorTeleporterInstance>();
			target.GetOrAddComponent<TeleporterVisualizer>();

			instance.Detach();
			target.transform.GetChild(0).position = data.startPosition;
			target.transform.GetChild(1).position = data.endPosition;
			instance.UpdateAttachments();
		}
	}

	public class EditorTeleporterInstance : MonoBehaviour
	{
		private List<MapObjectAnchor> anchors;

		private void Awake()
		{
			this.anchors = this.gameObject.GetComponentsInChildren<MapObjectAnchor>().ToList();
		}

		public MapObjectAnchor GetAnchor(int index)
		{
			return this.anchors[index];
		}

		public void UpdateAttachments()
		{
			foreach (var anchor in this.anchors)
			{
				anchor.UpdateAttachment();
			}
		}

		public void Detach()
		{
			foreach (var anchor in this.anchors)
			{
				anchor.Detach();
			}
		}
	}
}