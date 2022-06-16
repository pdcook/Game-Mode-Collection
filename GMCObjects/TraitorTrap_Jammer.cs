using System.Collections;
using UnityEngine;
using GameModeCollection.GameModes;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.TRT.VoiceChat;

namespace GameModeCollection.GMCObjects
{
    /// <summary>
    /// Traitor trap which disables proximity VC (but not intercom or traitor chat) for a specified amount of time (30 seconds)
    /// Can only be interacted with at close range and requires LoS
    /// </summary>
    public class TraitorTrap_Jammer : Interactable
    {

        public const float JamDuration = 30f;

        public override string HoverText { get; protected set; } = "Traitor Trap\n(Jam Comms)";
        public override Color TextColor { get; protected set; } = GM_TRT.DullWhite;
        public override Color IconColor { get; protected set; } = GM_TRT.TraitorColor;
        public override bool InteractableInEditor { get; protected set; } = false;
        public override Alignment? RequiredAlignment { get; protected set; } = Alignment.Traitor;
        public override float VisibleDistance { get; protected set; } = 5f;
        public override bool RequireLoS { get; protected set; } = true;

        public override void OnInteract(Player player)
        {
            // play a static sound?

            // jam comms for 30 seconds
            TRTProximityChannel.JamCommsFor(JamDuration);

            // "destroy" this object, since jamming can only be done once
            this.gameObject.SetActive(false);
        }
    }
}
