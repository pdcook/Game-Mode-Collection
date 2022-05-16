using System;
using System.Linq;
using UnityEngine;
using GameModeCollection.GameModes.TRT;
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
        public static Player GetClosestOtherPlayer(this PlayerManager instance, Player thisPlayer, bool requireAlive = true, bool requireLoS = false)
        {
            Player closest = null;
            float dist = float.PositiveInfinity;
            foreach (Player player in instance.players)
            {
                if (player.playerID == thisPlayer.playerID) { continue; }
                if (requireAlive && player.data.dead) { continue; }
                if (requireLoS && !instance.CanSeePlayer(thisPlayer.data.playerVel.position, player).canSee) { continue; }
                if (Vector2.Distance(player.data.playerVel.position, thisPlayer.data.playerVel.position) < dist) { closest = player; }
            }
            return closest;
        }
        public static Player GetClosestCorpse(this PlayerManager instance, Player thisPlayer, bool requireLoS = false)
        {
            Player closest = null;
            float dist = float.PositiveInfinity;
            foreach (Player player in instance.players)
            {
                if (player.playerID == thisPlayer.playerID) { continue; }
                if (!player.data.dead || player.GetComponent<HealthHandlerExtensions.Corpse>() is null) { continue; }
                if (requireLoS && !instance.CanSeePlayer(thisPlayer.data.playerVel.position, player).canSee) { continue; }
                if (Vector2.Distance(player.data.playerVel.position, thisPlayer.data.playerVel.position) < dist) { closest = player; }
            }
            return closest;
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
