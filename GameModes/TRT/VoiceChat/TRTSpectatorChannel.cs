using RoundsVC;
using UnityEngine;
using GameModeCollection.Objects;
using GameModeCollection.GameModeHandlers;
using UnboundLib.GameModes;
using RoundsVC.VoiceChannels;

namespace GameModeCollection.GameModes.TRT.VoiceChat
{
    public class TRTSpectatorChannel : VoiceChannel
    {
        public override int ChannelID { get; } = 600; // TRT channels start at 600
        public override int Priority { get; } = 100; // TRT priorities start at 100
        public override string ChannelName { get; } = "Spectators";
        public override Color ChannelColor { get; } = GM_TRT.DullWhite;
        public override bool GlobalUIIconsEnabled => true;
        public override bool LocalUIIconsEnabled => true;
        public override AudioFilters AudioFilters { get; } = AudioFilters.None;

        public override float RelativeVolume(Player speaking, Player listening)
        {
            if (GM_TRT.instance.CurrentPhase == GM_TRT.RoundPhase.PostBattle) { return 1f; }
            if (speaking != null && !speaking.data.dead) { return 0f; }
            if (listening != null && !listening.data.dead) { return 0f; }
            return 1f;
        }

        public override bool SpeakingEnabled(Player player)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return false; }
            if (player is null || player.data.dead || GM_TRT.instance.CurrentPhase == GM_TRT.RoundPhase.PostBattle) { return true; }
            else { return false; }
        }
    }
}
