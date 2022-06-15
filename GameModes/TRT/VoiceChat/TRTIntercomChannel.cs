using RoundsVC;
using UnityEngine;
using GameModeCollection.Objects;
using GameModeCollection.GameModeHandlers;
using UnboundLib.GameModes;
using RoundsVC.VoiceChannels;

namespace GameModeCollection.GameModes.TRT.VoiceChat
{
    public class TRTIntercomChannel : VoiceChannel
    {
        public override int ChannelID { get; } = 603; // TRT channels start at 600
        public override int Priority { get; } = 103; // TRT priorities start at 100
        public override string ChannelName { get; } = "Intercom";
        public override Color ChannelColor { get; } = GM_TRT.DullWhite;
        public override AudioFilters AudioFilters { get; } = new AudioFilters(reverb: AudioReverbPreset.Hangar, highPassCutoff: 500, distortion: 0.5f);

        public override float RelativeVolume(Player speaking, Player listening)
        {
            return 1f;
        }

        public override bool SpeakingEnabled(Player player)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return false; }
            if (player?.data?.isSilenced ?? true) { return false; } // silenced players cannot speak

            // TODO: players can use this channel if they are using the intercom map object
            return false;
        }
    }
}
