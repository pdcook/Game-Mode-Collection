using System.Collections;
using UnityEngine;
using GameModeCollection.GameModes;
using GameModeCollection.GameModes.TRT;

namespace GameModeCollection.GMCObjects
{
    /// <summary>
    /// The main behaviour for the traitor door, which can be opened by traitors, and closes automatically after a brief period of time.
    /// Traitors can see the interact icon from all distances, as they stay on the edge of the screen similar to Radar points
    /// </summary>
    public class TraitorTrap_Destructible : Interactable
    {
        /// <summary>
        /// Creates a new gameobject with the traitor interaction assigned to this door
        /// </summary>
        public override string HoverText { get; protected set; } = "Traitor Trap\n(Destroy)";
        public override Color TextColor { get; protected set; } = GM_TRT.DullWhite;
        public override Color IconColor { get; protected set; } = GM_TRT.TraitorColor;
        public override bool InteractableInEditor { get; protected set; } = false;
        public override Alignment? RequiredAlignment { get; protected set; } = Alignment.Traitor;

        public override void OnInteract(Player player)
        {
            // play a breaking sound?

            // "destroy" this object
            this.gameObject.SetActive(false);
        }
    }
}
