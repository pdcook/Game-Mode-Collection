using RoundsVC;
using UnityEngine;
using GameModeCollection.Objects;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes.TRT;
using UnboundLib.GameModes;
using GameModeCollection.Extensions;
using RoundsVC.VoiceChannels;

namespace GameModeCollection.GameModes.TRT.VoiceChat
{
    public class TRTTraitorChannel : VoiceChannel
    {
        public override int ChannelID { get; } = 602; // TRT channels start at 600
        public override int Priority { get; } = 102; // TRT priorities start at 100
        public override string ChannelName { get; } = "Traitors";
        public override Color ChannelColor { get; } = GM_TRT.TraitorColor;
        public override AudioFilters AudioFilters { get; } = new AudioFilters(highPassCutoff: 500, distortion: 0.5f);


        public override float RelativeVolume(Player speaking, Player listening)
        {
            // players can only hear this channel if:
            /*
             * - the current gamemode is TRT
             * - they are a traitor
             * - they are alive
             * 
             */
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return 0f; }
            if (listening is null || listening.data.dead) { return 0f; }
            Alignment? alignment = RoleManager.GetPlayerAlignment(listening);
            if (alignment != Alignment.Traitor)
            {
                return 0f;
            }
            return 1f;
        }

        public override bool SpeakingEnabled(Player player)
        {
            // players can only speak in this channel if:
            /*
             * - the current gamemode is TRT
             * - they are a traitor or chaos (jester)
             * - they are alive
             * - and they are holding the PTT key
             * 
             */

            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return false; }
            if (player is null || player.data.dead) { return false; }
            if (player.data?.isSilenced ?? true) { return false; } // silenced players cannot speak
            Alignment? alignment = RoleManager.GetPlayerAlignment(player);
            if (alignment != Alignment.Traitor && alignment != Alignment.Chaos)
            {
                return false;
            }
            if (!player.data.playerActions.TraitorPTTIsHeld())
            {
                return false;
            }
            return true;
        }
    }
}
