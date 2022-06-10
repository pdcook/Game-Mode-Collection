using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;
namespace GameModeCollection.GameModes.TRT
{
    public static class RoundSummary
    {
        /// The class which handles:
        /// - tracking the info of the current round
        /// - displaying round summaries
        /// - managing RoundEvents

        /// Tracked Information:
        /// - all kills (killer and victim)
        /// - all player damage (source, target, amount)
        /// - custom events registered by key

        // kills by each playerID (kills[killerID] = {victimID, victimID, ...})
        private static Dictionary<int, List<int>> Kills = new Dictionary<int, List<int>>() { };
        public static List<int> GetPlayerKills(int playerID)
        {
            if (Kills.ContainsKey(playerID))
            {
                return Kills[playerID];
            }
            return new List<int>();
        }
        // teamkills by each playerID (TeamKills[killerID] = {victimID, victimID, ...})
        private static Dictionary<int, List<int>> TeamKills = new Dictionary<int, List<int>>() { };
        public static List<int> GetPlayerTeamKills(int playerID)
        {
            if (TeamKills.ContainsKey(playerID))
            {
                return TeamKills[playerID];
            }
            return new List<int>();
        }
        // EnemyKills by each playerID (EnemyKills[killerID] = {victimID, victimID, ...})
        private static Dictionary<int, List<int>> EnemyKills = new Dictionary<int, List<int>>() { };
        public static List<int> GetPlayerEnemyKills(int playerID)
        {
            if (EnemyKills.ContainsKey(playerID))
            {
                return EnemyKills[playerID];
            }
            return new List<int>();
        }

        // damage from each playerID (DamageDealt[playerID] = amount)
        private static Dictionary<int, float> DamageDealt = new Dictionary<int, float>() { };
        public static float GetPlayerDamageDealt(int playerID)
        {
            if (DamageDealt.ContainsKey(playerID))
            {
                return DamageDealt[playerID];
            }
            return 0;
        }
        // damage to each playerID (DamageTaken[playerID] = amount)
        private static Dictionary<int, float> DamageTaken = new Dictionary<int, float>() { };
        public static float GetPlayerDamageTaken(int playerID)
        {
            if (DamageTaken.ContainsKey(playerID))
            {
                return DamageTaken[playerID];
            }
            return 0;
        }
        // damage from each playerID to enemies (DamageDealtToEnemies[playerID] = amount)
        private static Dictionary<int, float> DamageDealtToEnemies = new Dictionary<int, float>() { };
        public static float GetPlayerDamageDealtToEnemies(int playerID)
        {
            if (DamageDealtToEnemies.ContainsKey(playerID))
            {
                return DamageDealtToEnemies[playerID];
            }
            return 0;
        }
        // damage from each playerID to allies (DamageDealtToAllies[playerID] = amount)
        private static Dictionary<int, float> DamageDealtToAllies = new Dictionary<int, float>() { };
        public static float GetPlayerDamageDealtToAllies(int playerID)
        {
            if (DamageDealtToAllies.ContainsKey(playerID))
            {
                return DamageDealtToAllies[playerID];
            }
            return 0;
        }

        // custom events by key (Events[playerID][key] = object)
        private static Dictionary<int, Dictionary<string, object>> Events = new Dictionary<int, Dictionary<string, object>>() { };
        public static T GetPlayerEvent<T>(int playerID, string key) where T : class
        {
            if (Events.ContainsKey(playerID))
            {
                if (Events[playerID].ContainsKey(key))
                {
                    return (T)Events[playerID][key];
                }
            }
            return null;
        }

        public static void LogDamage(Player source, Player receiver, float damage)
        {
        }
        public static void LogKill(Player source, Player receiver)
        {

        }
        public static void LogEvent(string key, params object[] args)
        {

        }
    }
}
