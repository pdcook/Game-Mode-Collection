using System.Collections;
using UnityEngine;
using GameModeCollection.GameModes;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.TRT.VoiceChat;
using Sonigon;
using Sonigon.Internal;
using UnboundLib;
using UnboundLib.GameModes;

namespace GameModeCollection.GMCObjects
{
    /// <summary>
    /// Traitor trap which disables proximity VC (but not intercom or traitor chat) for a specified amount of time (30 seconds)
    /// Can only be interacted with at close range and requires LoS
    /// </summary>
    public class TraitorTrap_Jammer : Interactable
    {

        public static IEnumerator ForceStop(IGameModeHandler gm)
        {
            if (GameModeManager.CurrentHandlerID == TRTHandler.GameModeID)
            {
                if (StaticPlaying)
                {
                    StaticPlaying = false;
                    SoundManager.Instance.Stop(Static, SoundManager.Instance.GetTransform(), true);
                }
                TRTProximityChannel.ForceUnjamComms();
            }

            yield break;
        }

        public const float JamDuration = 30f;

        public override string HoverText { get; protected set; } = "Traitor Trap\n(Jam Comms)";
        public override Color TextColor { get; protected set; } = GM_TRT.DullWhite;
        public override Color IconColor { get; protected set; } = GM_TRT.TraitorColor;
        public override bool InteractableInEditor { get; protected set; } = false;
        public override Alignment? RequiredAlignment { get; protected set; } = Alignment.Traitor;
        public override float VisibleDistance { get; protected set; } = 5f;
        public override bool RequireLoS { get; protected set; } = true;

        private const float StaticVol = 0.5f;

        private static bool StaticPlaying = false;
        private static SoundEvent _Static = null;
        public static SoundEvent Static
        {
            get
            {
                if (_Static is null)
                {
                    AudioClip sound = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("Static.ogg");
                    SoundContainer soundContainer = ScriptableObject.CreateInstance<SoundContainer>();
                    soundContainer.setting.volumeIntensityEnable = true;
                    soundContainer.setting.loopEnabled = true;
                    soundContainer.audioClip[0] = sound;
                    _Static = ScriptableObject.CreateInstance<SoundEvent>();
                    _Static.soundContainerArray[0] = soundContainer;
                }
                return _Static;
            }
        }

        public override void OnInteract(Player player)
        {
            // play a static sound for the duration of the jam
            if (!StaticPlaying)
            {
                SoundManager.Instance.Play(Static, SoundManager.Instance.GetTransform(), new SoundParameterBase[] { new SoundParameterIntensity(Optionshandler.vol_Master * Optionshandler.vol_Sfx * StaticVol) });
                StaticPlaying = true;

                // start a coroutine to stop the static sound after the jam duration
                // can't be on this object since it becomes inactive immediately
                GM_TRT.instance.ExecuteAfterSeconds(JamDuration, () =>
                {
                    StaticPlaying = false;
                    SoundManager.Instance.Stop(Static, SoundManager.Instance.GetTransform(), true);
                });
            }

            // jam comms for 30 seconds
            TRTProximityChannel.JamCommsFor(JamDuration);

            // "destroy" this object, since jamming can only be done once
            this.gameObject.SetActive(false);
        }
    }
}
