using System.Collections;
using UnityEngine;
using GameModeCollection.GameModes;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.TRT.VoiceChat;
using UnboundLib.Utils;
using Sonigon;
using Sonigon.Internal;

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

        private TimeSince IC_Timer;
        private TimeSince CheckPlayer_Timer;
        private bool broadcasting = false;
        private bool recharging = false;

        private static SoundEvent _IC_Start = null;
        private static SoundEvent _IC_Stop = null;
        private const float SFX_Vol = 1f;
        private Player IntercomPlayer = null;

        private const float CheckPlayerEvery = 0.1f;

        public static SoundEvent IC_Start
        {
            get
            {
                if (_IC_Start is null)
                {
                    AudioClip sound = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("IntercomStart.ogg");
                    SoundContainer soundContainer = ScriptableObject.CreateInstance<SoundContainer>();
                    soundContainer.setting.volumeIntensityEnable = true;
                    soundContainer.audioClip[0] = sound;
                    _IC_Start = ScriptableObject.CreateInstance<SoundEvent>();
                    _IC_Start.soundContainerArray[0] = soundContainer;
                }
                return _IC_Start;
            }
        }
        public static SoundEvent IC_Stop
        {
            get
            {
                if (_IC_Stop is null)
                {
                    AudioClip sound = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("IntercomStop.ogg");
                    SoundContainer soundContainer = ScriptableObject.CreateInstance<SoundContainer>();
                    soundContainer.setting.volumeIntensityEnable = true;
                    soundContainer.audioClip[0] = sound;
                    _IC_Stop = ScriptableObject.CreateInstance<SoundEvent>();
                    _IC_Stop.soundContainerArray[0] = soundContainer;
                }
                return _IC_Stop;
            }
        }

        private void SetIntercomPlayer(Player player)
        {
            this.IntercomPlayer = player;
            TRTIntercomChannel.SetIntercomPlayer(player);
        }

        public override void OnInteract(Player player)
        {
            if (this.broadcasting || this.recharging) { return; }

            // play start sound
            SoundManager.Instance.PlayMusic(IC_Start, false, true, new SoundParameterBase[] { new SoundParameterIntensity(Optionshandler.vol_Master * Optionshandler.vol_Sfx * SFX_Vol) });

            // enable this player to speak in the Intercom VC for 30 seconds
            this.broadcasting = true;
            this.SetIntercomPlayer(player);
            this.IC_Timer = 0f;
            this.CheckPlayer_Timer = 0f;

        }
        public void StartRecharge()
        {
            this.InteractionUI.SetText("<size=50%>Recharging...");
            this.IC_Timer = RechargeTime;
        }
        void Update()
        {
            if (this.broadcasting)
            {
                this.InteractionUI.SetText($"<b>Broadcasting...</b>\n<b>{CommDuration - this.IC_Timer.Relative:N0}</b>");
                this.InteractionUI.SetTextColor(BroadcastingColor);
                bool stop = this.IC_Timer > CommDuration || this.IntercomPlayer is null || this.IntercomPlayer.data.dead;
                if (!stop && this.CheckPlayer_Timer > CheckPlayerEvery)
                {
                    // check that the player is still within range
                    stop = Vector2.Distance(this.IntercomPlayer.transform.position, this.transform.position) > this.VisibleDistance // the player is too far
                            || (
                                this.RequireLoS // or (there is a line of sight requirement
                                && !PlayerManager.instance.CanSeePlayer(this.transform.position, this.IntercomPlayer).canSee // AND the player cannot see the interactable)
                               );
                    this.CheckPlayer_Timer = 0f;

                }
                if (stop)
                {
                    this.broadcasting = false;
                    this.recharging = true;
                    this.SetIntercomPlayer(null);
                    // play stop sound
                    SoundManager.Instance.PlayMusic(IC_Stop, false, true, new SoundParameterBase[] { new SoundParameterIntensity(Optionshandler.vol_Master * Optionshandler.vol_Sfx * SFX_Vol) });
                    this.IC_Timer = 0f;
                }
            }
            else if (this.recharging)
            {
                this.InteractionUI.SetText($"<size=50%>Recharging...\n{RechargeTime - this.IC_Timer.Relative:N0}");
                this.InteractionUI.SetTextColor(RechargingColor);
                if (this.IC_Timer > RechargeTime)
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
