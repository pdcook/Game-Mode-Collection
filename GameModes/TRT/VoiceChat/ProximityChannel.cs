using RoundsVC;
using UnityEngine;
using GameModeCollection.Objects;
using GameModeCollection.GameModeHandlers;
using UnboundLib.GameModes;
using RoundsVC.VoiceChannels;

namespace GameModeCollection.GameModes.TRT.VoiceChat
{
    public class ProximityChannel : VoiceChannel
    {
        public override int ChannelID { get; } = 601; // TRT channels start at 600
        public override int Priority { get; } = 101; // TRT priorities start at 100
        public override string ChannelName { get; } = "General Proximity";
        public override Color ChannelColor { get; } = GM_TRT.InnocentColor;
        public override AudioFilters AudioFilters { get; } = AudioFilters.None;

        // volume is a function of distance, walls between the players, max distance, and min distance
        private const float MinDistance = 10f;
        private const float MaxDistance = 60f;
        private const float CutoffDistance = 60f; // distance at which players cannot hear eachother at all
        private const float WallPenaltyPercent = 0.5f;
        private const int MaxWallsCutoff = 5; // number of walls between players after which players cannot hear eachother at all

        public override SpatialEffects SpatialEffects { get; } = 
            new SpatialEffects(
                true,
                AudioRolloffMode.Logarithmic,
                true,
                MinDistance, 
                MaxDistance,
                0f, 
                SpatialEffects.LogarithmicBlend(MinDistance)
                    );

        public int GetWallsBetweenPlayers(Player player1, Player player2)
        {
            int walls = 0;
            RaycastHit2D[] array = Physics2D.RaycastAll(player1.data.playerVel.position, (player2.data.playerVel.position - (Vector2)player1.data.playerVel.position).normalized, Vector2.Distance(player1.data.playerVel.position, player2.data.playerVel.position), PlayerManager.instance.canSeePlayerMask);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].transform
                    && !array[i].transform.root.GetComponent<SpawnedAttack>()
                    && !array[i].transform.root.GetComponent<Player>()
                    && !array[i].transform.root.GetComponentInChildren<PhysicsItem>()
                    )
                {
                    walls++;
                }
            }
            return walls;
        }

        public override float RelativeVolume(Player speaking, Player listening)
        {
            if (speaking is null) { return 0f; }
            if (listening is null) { return 1f; }
            float distance = Vector2.Distance(speaking.data.playerVel.position, listening.data.playerVel.position);
            if (distance > CutoffDistance) { return 0f; }
            int walls = this.GetWallsBetweenPlayers(speaking, listening);
            if (walls > MaxWallsCutoff) { return 0f; }
            return UnityEngine.Mathf.Pow(WallPenaltyPercent, walls);
        }

        public override bool SpeakingEnabled(Player player)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return false; }
            if (player is null || player.data.dead) { return false; }
            else { return true; }
        }
    }
}
