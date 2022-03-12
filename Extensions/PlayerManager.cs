using System;
using System.Linq;
namespace GameModeCollection.Extensions
{
    static class PlayerManagerExtensions
    {
        public static Player GetPlayerWithID(this PlayerManager instance, int playerID)
        {
            return instance.players.FirstOrDefault(p => p.playerID == playerID);
        }
        public static Player GetLocalPlayer(this PlayerManager instance)
        {
            return instance.players.FirstOrDefault(p => p.data.view.IsMine);
        }
        public static void SetPlayersInvulnerableAndIntangible(this PlayerManager instance, bool inv)
        {
            foreach (Player player in instance.players)
            {
                player.data.healthHandler.SetInvulnerable(inv);
                player.data.healthHandler.SetIntangible(inv);
            }
        }
        public static void SetPlayersInvulnerable(this PlayerManager instance, bool invulnerable)
        {
            foreach (Player player in instance.players)
            {
                player.data.healthHandler.SetInvulnerable(invulnerable);
            }
        }
        public static void SetPlayersIntangible(this PlayerManager instance, bool intangible)
        {
            foreach (Player player in instance.players)
            {
                player.data.healthHandler.SetIntangible(intangible);
            }
        }
        public static void ResetKarma(this PlayerManager instance)
        {
            foreach (Player player in instance.players)
            {
                player.data.TRT_ResetKarma();
            }
        }
        public static void ForEachPlayer(this PlayerManager instance, Action<Player> action)
        {
            foreach (Player player in instance.players)
            {
                action(player);
            }
        }
        public static void ForEachAlivePlayer(this PlayerManager instance, Action<Player> action)
        {
            foreach (Player player in instance.players.Where(p => !p.data.dead))
            {
                action(player);
            }
        }
        public static void ForEachDeadPlayer(this PlayerManager instance, Action<Player> action)
        {
            foreach (Player player in instance.players.Where(p => p.data.dead))
            {
                action(player);
            }
        }
    }
}
