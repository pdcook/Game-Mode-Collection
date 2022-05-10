using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameModeCollection.GMCObjects
{
    /// <summary>
    /// The main behaviour for the traitor door, which can be opened by traitors, and closes automatically after a brief period of time.
    /// Traitors can see the interact icon from all distances, as they stay on the edge of the screen similar to Radar points
    /// </summary>
    public class TraitorDoor : MonoBehaviour
    {
        GameObject Interacter { get; set; }
        /// <summary>
        /// Creates a new gameobject with the traitor interaction assigned to this door
        /// </summary>
        void Start()
        {
            var interact = new GameObject("TraitorInteract");
            interact.transform.parent = transform;
            interact.transform.localPosition = Vector3.zero;
            interact.transform.localRotation = Quaternion.identity;
            interact.transform.localScale = Vector3.one;
            interact.AddComponent<TraitorInteractable>();
            Interacter = interact;
            
        }
    }
}
