using System.Collections;
using UnityEngine;
using GameModeCollection.GameModes;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.TRT.VoiceChat;
using UnboundLib.Utils;

namespace GameModeCollection.GMCObjects
{
    /// <summary>
    /// Intercom map object (similar to that found in SCP secret laboratory), can be used once every so often
    /// to broadcast VC to the entire lobby for a short time
    /// </summary>
    public class TRT_Intercom : Interactable
    {

        public const float CommDuration = 30f;
        public const float RechargeTime = 90f;

        public override string HoverText { get; protected set; } = "Intercom";
        public override Color TextColor { get; protected set; } = GM_TRT.DullWhite;
        public override Color IconColor { get; protected set; } = GM_TRT.DetectiveColor;
        public static readonly Color RechargingColor = new Color32(230, 230, 230, 128);
        public static readonly Color BroadcastingColor = GM_TRT.InnocentColor;
        public override bool InteractableInEditor { get; protected set; } = false;
        public override Alignment? RequiredAlignment { get; protected set; } = null;
        public override float VisibleDistance { get; protected set; } = 5f;
        public override bool RequireLoS { get; protected set; } = true;

        private TimeSince timer;
        private bool broadcasting = false;
        private bool recharging = false;

        public override void OnInteract(Player player)
        {
            if (this.broadcasting || this.recharging) { return; }

            // play start sound

            // enable this player to speak in the Intercom VC for 30 seconds
            this.broadcasting = true;
            TRTIntercomChannel.SetIntercomPlayer(player);
            this.timer = 0f;

        }
        public void StartRecharge()
        {
            this.InteractionUI.SetText("<size=50%>Recharging...");
            this.timer = RechargeTime;
        }
        void Update()
        {
            if (this.broadcasting)
            {
                this.InteractionUI.SetText($"<b>Broadcasting...</b>\n<b>{CommDuration - this.timer.Relative:N0}</b>");
                this.InteractionUI.SetTextColor(BroadcastingColor);
                if (this.timer > CommDuration)
                {
                    this.broadcasting = false;
                    this.recharging = true;
                    TRTIntercomChannel.SetIntercomPlayer(null);
                    // play end sound
                    this.timer = 0f;
                }
            }
            else if (this.recharging)
            {
                this.InteractionUI.SetText($"<size=50%>Recharging...\n{RechargeTime - this.timer.Relative:N0}");
                this.InteractionUI.SetTextColor(RechargingColor);
                if (this.timer > RechargeTime)
                {
                    this.InteractionUI.SetText(this.HoverText);
                    this.InteractionUI.SetTextColor(this.TextColor);
                    this.recharging = false;
                }
            }
            else
            {
                this.InteractionUI.SetText(this.HoverText);
                this.InteractionUI.SetTextColor(this.TextColor);
            }
        }
    }
}
