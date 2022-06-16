using RoundsVC;
using UnityEngine;
using GameModeCollection.Objects;
using GameModeCollection.GameModeHandlers;
using UnboundLib.GameModes;
using RoundsVC.VoiceChannels;
using UnboundLib.Networking;
using UnboundLib;

namespace GameModeCollection.GameModes.TRT.VoiceChat
{
    public class TRTIntercomChannel : VoiceChannel
    {
        public override int ChannelID { get; } = 603; // TRT channels start at 600
        public override int Priority { get; } = 103; // TRT priorities start at 100
        public override string ChannelName { get; } = "Intercom";
        public override Color ChannelColor { get; } = GM_TRT.DullWhite;
        public override AudioFilters AudioFilters { get; } = new AudioFilters(reverb: AudioReverbPreset.Hangar, highPassCutoff: 500, distortion: 0.5f);

        public static int IntercomPlayerID { get; private set; } = -1; // playerID of the player who is currently speaking in the intercom

        public override float RelativeVolume(Player speaking, Player listening)
        {
            return 1f;
        }

        public override bool SpeakingEnabled(Player player)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return false; }
            if (player?.data?.isSilenced ?? true) { return false; } // silenced players cannot speak
            if (player.playerID != IntercomPlayerID) { return false; }

            return true;
        }

        public static void SetIntercomPlayer(Player player)
        {
            if (player == null) { IntercomPlayerID = -1; }
            else { IntercomPlayerID = player.playerID; }
        }
    }
}
