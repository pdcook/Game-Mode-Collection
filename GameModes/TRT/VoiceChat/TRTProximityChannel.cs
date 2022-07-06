using RoundsVC;
using UnityEngine;
using GameModeCollection.Objects;
using GameModeCollection.GameModeHandlers;
using UnboundLib.GameModes;
using RoundsVC.VoiceChannels;
using System.Collections;
using GameModeCollection.Utils;

namespace GameModeCollection.GameModes.TRT.VoiceChat
{
    public class TRTProximityChannel : VoiceChannel
    {
        public override int ChannelID { get; } = 601; // TRT channels start at 600
        public override int Priority { get; } = 101; // TRT priorities start at 100
        public override string ChannelName { get; } = "General Proximity";
        public override Color ChannelColor { get; } = new Color32(150, 150, 150, 255);
        public override bool GlobalUIIconsEnabled => false;
        public override bool LocalUIIconsEnabled => true;
        public override AudioFilters AudioFilters { get; } = AudioFilters.None;

        private static bool JamComms = false; // are the comms jammed?

        public override SpatialEffects SpatialEffects { get; } = 
            new SpatialEffects(
                true,
                Utils.GMCAudio.Rolloff,
                true,
                Utils.GMCAudio.MinDistance,
                Utils.GMCAudio.MaxDistance,
                0f, 
                SpatialEffects.LogarithmicBlend(Utils.GMCAudio.MinDistance)
                    );

        public override float RelativeVolume(Player speaking, Player listening)
        {
            if (GM_TRT.instance.CurrentPhase == GM_TRT.RoundPhase.PostBattle) { return 0f; }
            if (speaking is null) { return 0f; }
            if (listening is null || listening.data.dead) { return 0.5f; } // spectators can hear everyone at half volume
            float distance = Vector2.Distance(speaking.data.playerVel.position, listening.data.playerVel.position);
            if (distance > Utils.GMCAudio.CutoffDistance) { return 0f; }
            return Utils.GMCAudio.FalloffByWalls(speaking.data.playerVel.position, listening.data.playerVel.position);
        }

        public override bool SpeakingEnabled(Player player)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return false; }
            if (player is null || player.data.dead || GM_TRT.instance.CurrentPhase == GM_TRT.RoundPhase.PostBattle) { return false; }
            if (player.data?.isSilenced ?? true) { return false; } // silenced players cannot speak
            if (JamComms) { return false; } // comms are jammed
            else { return true; }
        }

        public static void JamCommsFor(float jamTime)
        {
            GM_TRT.instance.StartCoroutine(IJamCommsFor(jamTime));
        }
        private static IEnumerator IJamCommsFor(float jamTime)
        {
            JamComms = true;
            yield return new WaitForSeconds(jamTime);
            JamComms = false;
        }
        public static void ForceUnjamComms()
        {
            JamComms = false;
        }
    }
}
