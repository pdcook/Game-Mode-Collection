using System;
using System.Linq;
namespace GameModeCollection.Extensions
{
    static class PlayerManagerExtensions
    {
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
